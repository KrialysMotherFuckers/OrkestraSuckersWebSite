//using EFCore.BulkExtensions;
using Krialys.Common.Extensions;
using Krialys.Entities.COMMON;
using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.Common.Extensions;
using Krialys.Orkestra.Common.Models;
using Krialys.Orkestra.WebApi.Services.Common.Factories;
using Krialys.Orkestra.WebApi.Services.Data;
using Krialys.Orkestra.WebApi.Services.System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using SqlKata.Execution;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using System.Collections;
using System.Dynamic;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using static Krialys.Orkestra.Common.Shared.Logs;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Krialys.Orkestra.WebApi.Services.Common;

/// <summary>
/// Entity extensions
/// </summary>
public static class EntityExtensions
{
    #region private
    private const char separator = '.';

    /// <summary>
    /// var result = dataSource.SelectFromProperties("TRA_CODE", "TTL_RESULTAT");
    /// var count = test.OfType<object>().Count();
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entities"></param>
    /// <param name="propertyNames"></param>
    /// <returns></returns>
    private static IEnumerable SelectFromProperties<TEntity>(this IEnumerable entities, string[] propertyNames)
    {
        var param = Expression.Parameter(typeof(TEntity), "x");

        IList<Expression> propertyExpressions = propertyNames.Select(propertyName => propertyName
            .Split(separator)
            .Aggregate<string, Expression>(param, Expression.PropertyOrField)).ToList();

        var coalescedExpressions = new Expression[propertyExpressions.Count];

        for (int i = 0; i < propertyExpressions.Count; i++)
        {
            coalescedExpressions[i] = propertyExpressions[i].Type is { IsValueType: true, IsGenericTypeDefinition: true }
                ? Expression.Coalesce(
                    Expression.Convert(propertyExpressions[i], typeof(Nullable<>).MakeGenericType(propertyExpressions[i].Type)),
                    Expression.Constant(null, typeof(Nullable<>).MakeGenericType(propertyExpressions[i].Type)))
                : Expression.Convert(propertyExpressions[i], typeof(object));
        }

        Expression resultExpression = Expression.NewArrayInit(typeof(object), coalescedExpressions);

        foreach (object data in entities.OfType<TEntity>().Select(Expression.Lambda<Func<TEntity, object>>(resultExpression, param).Compile()))
            yield return data;
    }
    #endregion

    /// <summary>
    /// var el = dataSource.SelectExpandoFrom<TEntity>("TR_ID", "TRA_CODE");
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entities"></param>
    /// <param name="propertyNames"></param>
    /// <returns></returns>
    public static IEnumerable<ExpandoObject> SelectExpandoFrom<TEntity>(this IEnumerable entities, string[] propertyNames)
    {
        foreach (object[] el in entities.SelectFromProperties<TEntity>(propertyNames))
        {
            IDictionary<string, object> expandoDict = new ExpandoObject();

            for (int i = 0; i < propertyNames.Length; i++)
                expandoDict.Add(propertyNames[i], el[i]);

            yield return expandoDict as ExpandoObject;
        }
    }

    /// <summary>
    /// Convert DynamicTypeWrapper into ExpandoObject
    /// </summary>
    /// <param name="dynamicTypeWrapperEntities"></param>
    /// <returns></returns>
    public static IEnumerable<ExpandoObject> SelectExpando(this IEnumerable dynamicTypeWrapperEntities)
    {
        foreach (DynamicTypeWrapper el in dynamicTypeWrapperEntities)
            yield return el.Values.ToExpando();
    }

    /// <summary>
    /// Extension method that turns a dictionary of string and object to an ExpandoObject
    /// </summary>
    public static ExpandoObject ToExpando(this IDictionary<string, object> dictionary)
    {
        var expando = new ExpandoObject();
        var expandoDic = (IDictionary<string, object>)expando;

        foreach (var kvp in dictionary)
        {
            if (kvp.Value is IDictionary<string, object> nestedDictionary)
            {
                var expandoValue = nestedDictionary.ToExpando();
                expandoDic.Add(kvp.Key, expandoValue);
            }
            else if (kvp.Value is ICollection collection)
            {
                var itemList = new List<object>();

                foreach (var item in collection)
                {
                    if (item is IDictionary<string, object> nestedItem)
                    {
                        var expandoItem = nestedItem.ToExpando();
                        itemList.Add(expandoItem);
                    }
                    else
                    {
                        itemList.Add(item);
                    }
                }

                expandoDic.Add(kvp.Key, itemList);
            }
            else
            {
                expandoDic.Add(kvp);
            }
        }

        return expando;
    }

    /// <summary>
    /// Simplify property access in expressions
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static MemberExpression Property(this ParameterExpression parameter, string propertyName)
        => Expression.Property(parameter, propertyName);
}

/// <summary>
///   <para>
/// GENERIC CRUD ABSTRACT SERVICE FOR EACH SERVICE<br />VIRTUAL FUNCTIONS DEFAULT THE SERVICE
/// <br />Reference #1: <a href="https://docs.microsoft.com/fr-fr/ef/ef6/saving/transactions">Transactions</a>
/// <br />Reference #2: <a href="https://stackoverflow.com/questions/30675564/in-entity-framework-how-do-i-add-a-generic-entity-to-its-corresponding-dbset-wi">How-do-i-add-a-generic-entity-to-its-corresponding-dbset</a></para>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TContext"></typeparam>
public abstract class GenericCrud<TEntity, TContext> : ODataController
    where TEntity : class
    where TContext : DbContext
{
    private readonly TContext _dbContext;
    private readonly ITrackedEntitiesServices _trackedEntitiesServices;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISqlRaw _sqlRaw;
    private readonly DbSet<TEntity> _dbSet;
    private readonly Serilog.ILogger _logger;
    private readonly string _userId;
    private readonly string _uuid;
    private readonly QueryFactory _dbFac;

    /// <summary>
    /// Services related to labels (TETQ_ETIQUETTES).
    /// </summary>
    private readonly EtiquettesServices _etiquettesServices;

    protected GenericCrud(TContext dbContext,
        ITrackedEntitiesServices trackedEntitiesServices,
        IHttpContextAccessor httpContextAccessor,
        Serilog.ILogger logger,
        ISqlRaw sqlRaw,
        ISqliteQueryFactory sqliteFactory,
        EtiquettesServices etiquettesServices)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _trackedEntitiesServices = trackedEntitiesServices ?? throw new ArgumentNullException(nameof(trackedEntitiesServices));
        _httpContextAccessor = httpContextAccessor;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sqlRaw = sqlRaw ?? throw new ArgumentNullException(nameof(sqlRaw));
        _etiquettesServices = etiquettesServices ?? throw new ArgumentNullException(nameof(etiquettesServices));

        if (!string.IsNullOrEmpty(httpContextAccessor.HttpContext?.Request.Headers[Litterals.Authorization]))
        {
            // User Id comes from claims
            _userId = ((ClaimsIdentity)httpContextAccessor.HttpContext?.User.Identity)?.Claims
                .FirstOrDefault(x => x.Type.Equals(ClaimTypes.Name, StringComparison.OrdinalIgnoreCase))?.Value;

            // Session Id comes from startup and has been injected to Headers, ensuring
            _uuid = httpContextAccessor.HttpContext?.Request.Headers[Litterals.ApplicationClientSessionId].ToString();

            // This localizer relies on 'CultureInfo' from appsettings
            //_ = factory.Create(typeof(DataAnnotationsResources));
        }

        _dbSet = _dbContext.Set<TEntity>();
        if (!string.IsNullOrEmpty(_dbContext.Database.GetDbConnection().ConnectionString))
            _dbFac = sqliteFactory.Create(_dbContext.Database.GetDbConnection().ConnectionString);

        //// Example: log the compiled query to the console
        //_dbFac.Logger = result => {
        //    Console.WriteLine(result.Sql);
        //}; _dbFac.Query(typeof(TEntity).Name).Get<TEntity>();
    }

    private string GetClaimValue(string claimTypeName)
        => ((ClaimsIdentity)_httpContextAccessor.HttpContext?.User.Identity)?.Claims
        .FirstOrDefault(x => x.Type.Equals(claimTypeName, StringComparison.OrdinalIgnoreCase))?.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private IQueryable<TEntity> Conditions(IEnumerable<string> idsLookUp, bool findIn)
    {
        var dbSet = _dbSet.AsNoTracking();
        var dataList = Enumerable.Empty<TEntity>().AsQueryable();

        if (!dbSet.Any())
        {
            return dataList;
        }

        Entities.COMMON.DbFieldsExtensions.GetPkList<TEntity>().With(pi =>
        {
            dataList = pi.PropertyType switch
            {
                { } t when t == typeof(int) => dbSet.Where($"@0.Contains({pi.GetPkName()})",
                    idsLookUp.Select(x => Convert.ToInt32(x))),
                { } t when t == typeof(long) => dbSet.Where($"@0.Contains({pi.GetPkName()})",
                    idsLookUp.Select(x => Convert.ToInt64(x))),
                { } t when t == typeof(short) => dbSet.Where($"@0.Contains({pi.GetPkName()})",
                    idsLookUp.Select(x => Convert.ToInt16(x))),

                { } t when t == typeof(uint) => dbSet.Where($"@0.Contains({pi.GetPkName()})",
                    idsLookUp.Select(x => Convert.ToUInt32(x))),
                { } t when t == typeof(ulong) => dbSet.Where($"@0.Contains({pi.GetPkName()})",
                    idsLookUp.Select(x => Convert.ToUInt64(x))),
                { } t when t == typeof(ushort) => dbSet.Where($"@0.Contains({pi.GetPkName()})",
                    idsLookUp.Select(x => Convert.ToUInt16(x))),

                _ => dbSet.Where($"@0.Contains({pi.GetPkName()})", idsLookUp)
            };
        });

        //#if DEBUG
        //            // Check the generated SQL
        //            var replace = dataList.ToQueryString().Replace("[", "").Replace("]", "");
        //#endif

        return findIn
            ? dbSet.Intersect(dataList) // IN
            : dbSet.Except(dataList); // NOT IN

    }

    ///// <summary>
    ///// [CREATE] Insert a list of TEntity
    ///// </summary>
    ///// <param name="listItem"></param>
    ///// <returns></returns>
    public async ValueTask<string> Insert(IList<TEntity> listItems, bool modeBulk = false)
    {
        long count = 0;

        // Filter datas here
        var idsLookUp = listItems;

        await _dbContext.Database.CreateExecutionStrategy().Execute(async () =>
        {
            await _dbContext.Database.OpenConnectionAsync();
            await using var dbCTrans = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await dbCTrans.CreateSavepointAsync("Step_001");

                foreach (var item in idsLookUp)
                {
                    var x = await _dbSet.AddAsync(item);
                    if (x.State == EntityState.Added)
                        count++;
                }

                await _dbContext.SaveChangesAsync(true);

                await dbCTrans.CommitAsync();

                await _trackedEntitiesServices.AddTrackedEntitiesAsync(_dbContext.ChangeTracker,
                    new[] { typeof(TEntity).FullName }, Litterals.Insert, _userId, _uuid);
            }
            catch (Exception ex)
            {
                await dbCTrans.RollbackToSavepointAsync("Step_001");
                count = 0;
                CrudLogException(new LogException(GetType(), ex));
            }
        });

        // Gets last Id value generated by database
        if (count > 0)
            count = GetLastIdValue(idsLookUp, count);

        return count.ToString();
    }

    /// <summary>
    /// Gets last Id value
    /// </summary>
    /// <param name="listItems"></param>
    /// <returns></returns>
    private static long GetLastIdValue(IList<TEntity> listItems, long count)
    {
        try
        {
            var pkName = Entities.COMMON.DbFieldsExtensions.GetPkList<TEntity>().GetPkName();
            var value = listItems.AsQueryable().Select($"{pkName}").FirstOrDefault();

            return (value < 1) ? 0 : value;
        }
        catch
        {
            return count;
        }
    }

    /// <summary>
    /// READ - Get one or more data columns from a given entity (up to MaxTop items)
    /// </summary>
    /// <param name="propertyNames"></param>
    /// <returns></returns>
    public IEnumerable<ExpandoObject> SelectExpandoFrom(ODataQueryOptions<TEntity> queryOptions, string[] propertyNames)
    {
        IEnumerable<ExpandoObject> data = null;

        try
        {
            int maxTop = queryOptions.Top == null || queryOptions.Top.Value == 0 ? Globals.MaxTop : queryOptions.Top.Value;
            maxTop = maxTop < Globals.MaxTop ? maxTop : Globals.MaxTop;

            var dataSource = queryOptions.ApplyTo(_dbSet);

            // PropertyNames are ignored here
            if (dataSource is IEnumerable<DynamicTypeWrapper>)
            {
                data = queryOptions.Top == null
                    ? dataSource.SelectExpando().Take(maxTop)
                    : dataSource.SelectExpando();
            }
            // PropertyNames are mandatory here
            else if (dataSource is IEnumerable<TEntity>)
            {
                if (propertyNames == null)
                    propertyNames = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(e => e.Name).ToArray();

                data = queryOptions.Top == null
                    ? dataSource.SelectExpandoFrom<TEntity>(propertyNames).Take(maxTop)
                    : dataSource.SelectExpandoFrom<TEntity>(propertyNames);
            }
            // $expand and $select are not supported yet
            else if (dataSource.ElementType.Name.Equals("SelectAllAndExpand`1", StringComparison.Ordinal)
                || dataSource.ElementType.Name.Equals("SelectSome`1", StringComparison.Ordinal))
            {
                throw new NotSupportedException("$expand and/or $select are not supperted yet!");
            }

            return data ?? Enumerable.Empty<ExpandoObject>();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"< {nameof(GenericCrud<TEntity, TContext>)}: {nameof(SelectExpandoFrom)}, Error: {ex.Message}");

            IDictionary<string, object> expandoDict = new ExpandoObject();
            expandoDict.Add(Litterals.ApiCallException, ex.Message);

            return new List<ExpandoObject> { expandoDict as ExpandoObject };
        }
    }

    public IEnumerable GetEntitiesOrdered(IEnumerable enumerable, string[] propertyNames, bool isDescending = false)
        => GetEntitiesOrdered(enumerable.OfType<TEntity>().AsQueryable(), propertyNames, isDescending).AsEnumerable();

    private IQueryable<TEntity> GetEntitiesOrdered(IQueryable<TEntity> queryable, string[] propertyNames, bool isDescending = false)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var body = propertyNames.Aggregate<string, Expression>(parameter, Expression.Property);
        var lambda = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(body, typeof(object)), parameter);

        queryable = isDescending ? queryable.OrderByDescending(lambda) : queryable.OrderBy(lambda);

        return queryable;
    }

    private IEnumerable<IGrouping<object, TEntity>> GetEntitiesGrouped(IEnumerable enumerable, string[] groupByPropertyNames)
        => GetEntitiesGrouped(enumerable.OfType<TEntity>().AsQueryable(), groupByPropertyNames);

    private IQueryable<IGrouping<object, TEntity>> GetEntitiesGrouped(IQueryable<TEntity> queryable, string[] groupByPropertyNames)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var keySelector = groupByPropertyNames.Aggregate<string, Expression>(parameter, (current, propertyName) => Expression.Property(current, propertyName));
        var keyLambda = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(keySelector, typeof(object)), parameter);

        return queryable.GroupBy(keyLambda);
    }

    /// <summary>
    /// READ - compatible with Adaptors.UrlAdaptor + Adaptors.CustomAdaptor + ODATA ($expand and $select are supported as well)<br/>
    /// Example 1: SfDataManager Url="http://localhost:8000/api/mso/v1/TTL_LOGS/GetPagedQueryable" Adaptor="Adaptors.UrlAdaptor"<br/>
    /// Example 2: SfDataManager AdaptorInstance="typeof(ISfDataAdaptorServicesTTL_LOGS)" Adaptor="Adaptors.CustomAdaptor"<br/>
    /// https://xeppit.co.uk/articles/2019-11/using-syncfusion-blazor-datagrid<br/>
    /// Works ONLY with PARENT.CHILD relation, but not with PARENT.CHILD.ANCESTOR
    /// </summary>
    /// <param name="queryOptions">ODataQueryOptions</param>
    /// <param name="dm">DataManagerRequest</param>
    /// <returns>DataResult</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public DataResult<TEntity> GetPagedQueryable(ODataQueryOptions<TEntity> queryOptions, DataManagerRequest dm)
    {
        (IEnumerable Source, int Count) data = (default, default);

        try
        {
            data = (_dbSet.AsNoTracking(), _dbSet.Count()); // Open cursor, then total count (used for pagination)
        }
        catch (Exception ex)
        {
            data.Count = 0;
            CrudLogException(new LogException(GetType(), ex));
        }

        if (data.Count == 0)
        {
            return new DataResult<TEntity>
            {
                Result = Enumerable.Empty<TEntity>(),
                Count = 0
            };
        }

        bool modeOdata = false;

        // *** ODATA QUERY OPTIONS ***
        if (dm is null)
        {
            dm = new DataManagerRequest();
            modeOdata = !string.IsNullOrEmpty(queryOptions!.Request.QueryString.ToString());
        }
        else
        {
            // Are there parameters added to the data reader?
            if (dm.Params is not null)
            {
                modeOdata = dm.Params
                    .Where(x => x.Key.Equals(Litterals.OdataQueryParameters, StringComparison.Ordinal))
                    .Select(x => x.Value)
                    .FirstOrDefault() != null;

                // Get label filter parameter.
                if (dm.Params.TryGetValue(Litterals.FiltersEtq, out object filtersParam))
                {
                    // Get filtered labels.
                    var labels = _etiquettesServices.FilterEtq(filtersParam?.ToString());

                    data = (labels, labels.Count());
                }
            }
        }

        if (dm.Search is not null && dm.Search.Count > 0) // Searching
            data.Source = DataOperations.PerformSearching(data.Source, dm.Search);

        if (dm.Where is not null && dm.Where.Count > 0 && data.Source is not null) // Filtering using predicate
        {
            try // Special anti-con when wrong data type has been typed (like DateTime)
            {
                data.Source = DataOperations.PerformFiltering(data.Source, dm.Where, dm.Where[0].Operator);
            }
            catch (Exception ex)
            {
                return new DataResult<TEntity>
                {
                    Result = Enumerable.Empty<TEntity>(),
                    Count = 0
                };
            }
        }

        // *** COLUMN FILTERING ONLY // Ref: https://sqlkata.com/docs/ ***
        if (dm.Select is not null && dm.Select.Count == 1)
        {
            try
            {
                if (data is { Source: not null })
                {
                    if (!modeOdata)
                    {
                        var uniqueKeys = GetEntitiesGrouped(data.Source, dm.Select.ToArray())
                            ?.Select(group => group.Key)
                            .ToList();

                        if (uniqueKeys!.Count > 0)
                        {
                            var fieldName = dm.Select.First();
                            var groupByData = uniqueKeys?.Select(_ => Activator.CreateInstance(typeof(TEntity)) as TEntity).ToList();
                            var property = typeof(TEntity).GetProperty(fieldName);

                            // Update each groupByData element
                            for (var i = 0; i < uniqueKeys.Count; i++)
                                property?.SetValue(groupByData[i], uniqueKeys[i]);

                            var isDateType = typeof(DateTime).IsAssignableFrom(property.PropertyType)
                                          || typeof(DateTimeOffset).IsAssignableFrom(property.PropertyType)
                                          || (Nullable.GetUnderlyingType(property.PropertyType) != null
                                          && typeof(DateTime).IsAssignableFrom(Nullable.GetUnderlyingType(property.PropertyType)));

                            dm.Take = Globals.MaxTop / 10;

                            data.Source = isDateType
                                ? groupByData.OrderByDescending(e => property?.GetValue(e, null)).Take(dm.Take).ToList()
                                : groupByData.OrderBy(e => property?.GetValue(e, null)).Take(dm.Take).ToList();
                        }
                        else
                        {
                            data.Source = Enumerable.Empty<TEntity>();
                        }
                    }
                    else
                    {
                        // *** ODATA QUERY OPTIONS ***
                        data.Source = queryOptions!.ApplyTo(data.Source.AsQueryable()) as IQueryable<dynamic>;
                        data.Source = data.Source ?? Enumerable.Empty<TEntity>();

                        if (data.Source is IEnumerable<TEntity>)
                        {
                            data.Count = data.Source.OfType<TEntity>().Count();
                        }
                        else
                        {
                            data.Count = 0;

                            var enumerator = data.Source.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                var item = enumerator.Current;

                                // Manage $expand / $select
                                if (item.GetType().Name.Equals("SelectAllAndExpand`1", StringComparison.Ordinal)
                                    || item.GetType().Name.Equals("SelectSome`1", StringComparison.Ordinal))
                                {
                                    //resultList.Add((TEntity)item.GetType().GetProperty("Instance").GetValue(item));
                                    data.Count++;
                                }
                            }
                        }

                        // *** RETURN ODATA FILTERED VALUES
                        return new DataResult<TEntity>
                        {
                            Result = data.Source,
                            Count = data.Count
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                data.Count = 0;
                CrudLogException(new LogException(GetType(), ex));
            }

            if (data.Count == 0)
            {
                return new DataResult<TEntity>
                {
                    Result = Enumerable.Empty<TEntity>(),
                    Count = 0
                };
            }
        }

        // *** REGULAR ACTIONS ***
        if (dm.Select is not null && dm.Select.Count != 1)
        {
            if (dm.Sorted is not null && dm.Sorted.Count > 0) // Sorting
                data.Source = data.Source != null
                    ? DataOperations.PerformSorting(data.Source, dm.Sorted)
                    : Enumerable.Empty<TEntity>();
        }

        // *** ODATA QUERY OPTIONS ***
        if (modeOdata) // Apply ODATA query options
        {
            data.Source = data.Source != null
                ? queryOptions!.ApplyTo(data.Source.AsQueryable()) as IQueryable<dynamic>
                : Enumerable.Empty<TEntity>();

            data.Count = 0;

            if (data.Source is IEnumerable<TEntity>)
            {
                data.Count = data.Source.OfType<TEntity>().Count();
            }
            else
            {
                var enumerator = data.Source.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;

                    // Manage $expand / $select
                    if (item.GetType().Name.Equals("SelectAllAndExpand`1", StringComparison.Ordinal)
                        || item.GetType().Name.Equals("SelectSome`1", StringComparison.Ordinal))
                    {
                        //resultList.Add((TEntity)item.GetType().GetProperty("Instance").GetValue(item));
                        data.Count++;
                    }
                }
            }
        }
        else
        {
            if ((dm.Search is not null || dm.Where is not null) && data.Source is IEnumerable<TEntity>)
                data.Count = data.Source.OfType<TEntity>().Count();
        }

        if (data.Count == 0) // If there is no data, return empty data instead
        {
            return new DataResult<TEntity>
            {
                Result = Enumerable.Empty<TEntity>(),
                Count = 0
            };
        }

        if (dm.Skip != 0 && data.Source is not null) // Paging
            data.Source = DataOperations.PerformSkip(data.Source, dm.Skip);

        return new DataResult<TEntity>
        {
            Result = DataOperations.PerformTake(data.Source, dm.Take > 0 ? dm.Take : Globals.MaxTop),
            Count = data.Count
        };
    }

    /// <summary>
    /// READ using raw SQL, then reconstruct a new IDictionary
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public IList<IDictionary<string, object>> GetAllSqlRaw(string sql, ref int count)
        => _sqlRaw.GetAllSqlRaw(_dbContext, sql, ref count);

    /// <summary>
    /// Get Message and Stacktrace from an Exception
    /// </summary>
    /// <param name="ex">Exception</param>
    /// <returns>Tuple: (string Message, string[] StackEntries)</returns>
    private static (string Message, string[] StackEntries) GetErrorMessageAndStackEntries(Exception ex)
    {
        var stackTrace = ex.StackTrace
            ?.Replace("\n", "\t")
            .Replace("   at ", "")
            .Replace("\r", "")
            .Replace("\\", "/")
            .Trim();

        var stackEntries = stackTrace
            ?.Split("\t", StringSplitOptions.RemoveEmptyEntries)
            .Reverse()
            .ToArray();

        var message = ex.Message
            .Replace("\"", "");

        return (message, stackEntries ?? new string[1]);
    }

    /// <summary>
    /// UPDATE
    /// </summary>
    public async ValueTask<long> UpdateAsync(IList<TEntity> listItems, bool modeBulk)
    {
        long count = 0;
        await using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            try
            {
                _dbSet.UpdateRange(listItems);
                count = await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                await _trackedEntitiesServices.AddTrackedEntitiesAsync(
                                                    _dbContext.ChangeTracker,
                                                    new[] { typeof(TEntity).FullName },
                                                    Litterals.Update,
                                                    _userId,
                                                    _uuid);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                count = 0;
                CrudLogException(new LogException(GetType(), ex));
            }

        return count;
    }

    /// <summary>
    /// PATCH
    /// <br />Support for integer and string index types
    /// </summary>
    public async ValueTask<long> PatchEx(IEnumerable<object> ids, IEnumerable<byte[]> patchDocBytes)
    {
        long count = 0;

        await _dbContext.Database.CreateExecutionStrategy().Execute(async () =>
        {
            await _dbContext.Database.OpenConnectionAsync();
            await using var dbCTrans = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var listIds = ids.ToList();
                var listpatchDocBytes = patchDocBytes.ToList();

                if (listIds.Count > 0 && listpatchDocBytes.Count > 0)
                {
                    for (int i = 0; i < listIds.Count; i++)
                    {
                        switch (listIds[i])
                        {
                            case JsonElement id:
                                {
                                    var entity = id.ValueKind switch
                                    {
                                        JsonValueKind.Number => await _dbSet.FindAsync(id.GetInt32()),
                                        JsonValueKind.String => await _dbSet.FindAsync(id.GetString()),
                                        _ => throw new NotSupportedException("Type not currently supported")
                                    };

                                    if (entity == null)
                                    {
                                        continue;
                                    }

                                    await dbCTrans.CreateSavepointAsync("Step_001");
                                    var patchDoc = JsonConvert.DeserializeObject<JsonPatchDocument<TEntity>>(Encoding.UTF8.GetString(listpatchDocBytes[i]));
                                    patchDoc.ApplyTo(entity);
                                    ++count;

                                    break;
                                }
                        }
                    }

                    if (count == listIds.Count)
                    {
                        await _dbContext.SaveChangesAsync();

                        await dbCTrans.CommitAsync();

                        await _trackedEntitiesServices.AddTrackedEntitiesAsync(_dbContext.ChangeTracker,
                            new[] { typeof(TEntity).FullName }, Litterals.Patch, _userId, _uuid);
                    }
                }
            }
            catch (Exception ex)
            {
                await dbCTrans.RollbackToSavepointAsync("Step_001");
                count = 0;
                CrudLogException(new LogException(GetType(), ex));
            }
        });

        return count;
    }

    /// <summary>
    /// DELETE
    /// </summary>
    public async ValueTask<long> DeleteMany(byte[] ids, bool modeBulk)
    {
        long count = 0;

        switch (ids.Length)
        {
            case > 0:
                {
                    // Filter datas here
                    var dataList = Conditions(JsonSerializer.Deserialize<string[]>(ids), findIn: true);

                    await _dbContext.Database.CreateExecutionStrategy().Execute(async () =>
                    {
                        await _dbContext.Database.OpenConnectionAsync();
                        await using var dbCTrans = await _dbContext.Database.BeginTransactionAsync();

                        try
                        {
                            await dbCTrans.CreateSavepointAsync("Step_001");

                            _dbSet.RemoveRange(dataList);

                            count = await _dbContext.SaveChangesAsync();

                            await dbCTrans.CommitAsync();

                            await _trackedEntitiesServices.AddTrackedEntitiesAsync(_dbContext.ChangeTracker,
                                new[] { typeof(TEntity).FullName }, Litterals.Delete, _userId, _uuid);
                        }
                        catch (Exception ex)
                        {
                            await dbCTrans.RollbackToSavepointAsync("Step_001");
                            count = 0;
                            CrudLogException(new LogException(GetType(), ex));
                        }
                    });

                    break;
                }
        }

        return count;
    }

    /// <summary>
    /// @CrudLogException
    /// </summary>
    /// <param name="ex">Exception</param>
    private void CrudLogException(LogException logException)
    {
        // Identified user
        var userId = GetClaimValue(ClaimTypes.NameIdentifier);
        var userName = GetClaimValue(ClaimTypes.Name);

        // Fallback when no user has been found, then capture the User-Agent content
        if (string.IsNullOrEmpty(userName))
        {
            StringValues values = new();
            if ((_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("User-Agent", out values)).HasValue)
            {
                userId = "User-Agent";
                userName = values.Any() ? values.First() : null;
            }
        }

        // Log trace
        var logTypeName = $"@{nameof(CrudLogException)}";
        object dataIn = null;
        var stackTrace = logException.StackTrace?.Replace("\n", "\t").Replace("   at ", "").Trim();
        var stackEntries = stackTrace?.Split("\t", StringSplitOptions.RemoveEmptyEntries);
        object data = null;

        // Take data as it was given, don't try to encode
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        if (dataIn is not null)
        {
            // Can we serialize _data?
            data = dataIn is not string
                ? JsonSerializer.Serialize(dataIn, options)
                : dataIn;
        }
        else
        {
            // Is there any other data to serialize?
            if (logException.Data is not null)
                data = JsonSerializer.Serialize(logException.Data, options);
            //// Is there any stacktrace entries?
            //else if (stackEntries is not null)
            //    data = string.Join("; ", stackEntries.Reverse());
        }

        var datas = data?.ToString();
        if (!string.IsNullOrEmpty(datas))
        {
            datas = datas.Replace("\"", "'");
            datas = datas.Replace("\r", "■");
            datas = datas.Replace("\n", "■");
            datas = datas.Replace("■", " ").Trim();

            if (datas.StartsWith("'") && datas.EndsWith("'"))
                datas = datas.Replace("'", "");
        }

        var message = logException.Message;
        if (!string.IsNullOrEmpty(message))
        {
            message = message.Replace("\"", "'");
            message = message.ToString().Replace("\r", "■");
            message = message.ToString().Replace("\n", "■");
            message = message.ToString().Replace("■", " ").Trim();
        }

        var logType = $"{{{logTypeName}}}";

        _logger.Error(new ApplicationException(logException.Version),
            $"{logType}{(string.IsNullOrEmpty(message) ? "" : $", Reason: {message}")}",
            new LogExceptionEx
            {
                UserId = userId,
                UserName = userName,
                Message = message,
                StackTrace = stackEntries?.Last(),
                Source = logException.Source,
                FileName = logException.FileName,
                Action = logException.Action,
                AtLine = logException.AtLine,
                Data = datas,
                Version = logException.Version
            });
    }
}

public interface IGenericService<TEntity, TContext>
{
    void AddAsync([FromBody] CRUDModel<TEntity> record);
    Task<PagedResponse<TEntity>> GetAllAsync([FromBody] DataManagerRequest dm);
    void RemoveAsync([FromBody] CRUDModel<TEntity> record);
    void UpdateAsync([FromBody] CRUDModel<TEntity> record);
}

public class GenericServiceInjectHack<TEntity, TContext> : GenericService<TEntity, TContext>
    where TEntity : class
    where TContext : DbContext
{
    public GenericServiceInjectHack(TContext dbContext, ITrackedEntitiesServices trackedEntitiesServices, IHttpContextAccessor httpContextAccessor, Serilog.ILogger logger)
        : base(dbContext, trackedEntitiesServices, httpContextAccessor, logger) { }
}

public class GenericService<TEntity, TContext> : IGenericService<TEntity, TContext>
    where TEntity : class
    where TContext : DbContext
{
    private readonly TContext _dbContext;
    private readonly ITrackedEntitiesServices _trackedEntitiesServices;
    private readonly DbSet<TEntity> _dbCurrentSet;
    private readonly Serilog.ILogger _logger;
    private readonly string _userId;
    private readonly string _uuid;

    public GenericService(TContext dbContext, ITrackedEntitiesServices trackedEntitiesServices, IHttpContextAccessor httpContextAccessor,
        Serilog.ILogger logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _trackedEntitiesServices = trackedEntitiesServices ?? throw new ArgumentNullException(nameof(trackedEntitiesServices));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!string.IsNullOrEmpty(httpContextAccessor.HttpContext?.Request.Headers[Litterals.Authorization]))
        {
            // User Id comes from claims
            _userId = ((ClaimsIdentity)httpContextAccessor.HttpContext?.User.Identity)?.Claims
                .FirstOrDefault(x => x.Type.Equals(ClaimTypes.Name, StringComparison.OrdinalIgnoreCase))?.Value;

            // Session Id comes from startup and has been injected to Headers, ensuring
            _uuid = httpContextAccessor.HttpContext?.Request.Headers[Litterals.ApplicationClientSessionId].ToString();

            // This localizer relies on 'CultureInfo' from appsettings
            //_ = factory.Create(typeof(DataAnnotationsResources));
        }

        _dbCurrentSet = _dbContext.Set<TEntity>();
    }

    public async Task<PagedResponse<TEntity>> GetAllAsync([FromBody] DataManagerRequest dm)
        => await Task.FromResult(((IEnumerable)_dbCurrentSet.ToListAsync().Result).GetPagedResponse<TEntity>(dm));
    public void UpdateAsync([FromBody] CRUDModel<TEntity> record) => _dbCurrentSet.Update(record.Value);
    public void AddAsync([FromBody] CRUDModel<TEntity> record) => _dbCurrentSet.Add(record.Value);
    public void RemoveAsync([FromBody] CRUDModel<TEntity> record) => _dbCurrentSet.Remove(record.Value);
}