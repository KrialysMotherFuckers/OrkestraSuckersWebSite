using Krialys.Common.LZString;
using Krialys.Orkestra.Common;
using Krialys.Orkestra.Common.Models;
using Krialys.Orkestra.WebApi.Services.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using System.Dynamic;
using System.Net;
using System.Text.Json;

namespace Krialys.Orkestra.WebApi.Controllers.Common;

/// <summary>
///   <para>
/// GENERIC CRUD SERVICE FOR EACH CONTROLLER
/// <br />Reference #1: <a href="https://docs.microsoft.com/fr-fr/ef/ef6/saving/transactions">Transactions</a>
/// <br />Reference #2: <a href="https://stackoverflow.com/questions/30675564/in-entity-framework-how-do-i-add-a-generic-entity-to-its-corresponding-dbset-wi">How-do-i-add-a-generic-entity-to-its-corresponding-dbset</a></para>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TContext"></typeparam>
[Route("")]
[Authorize]
public abstract class GenericCrudController<TEntity, TContext> : ODataController
where TEntity : class
where TContext : DbContext
{
    #region PARAMETERS

    private readonly GenericCrud<TEntity, TContext> _service;

    protected GenericCrudController(GenericCrud<TEntity, TContext> service)
        => _service = service;

    #endregion PARAMETERS

    #region CRUD ENDPOINTS

    /// <summary>
    /// [CREATE]
    /// </summary>
    /// <param name="list"></param>
    /// <param name="modeBulk"></param>
    /// <returns>Odata</returns>
    [Produces(Litterals.ApplicationJson)]
    [HttpPost("[area]/[controller]/InsertAsync")]
    public async Task<ApiResult> InsertAsync([FromBody] byte[] list, [FromHeader(Name = "modebulk")] int modeBulk = 0)
    {
        try
        {
            var data = JsonSerializer.Deserialize<IList<TEntity>>(list);
            var c = await _service.Insert(data, modeBulk.Equals(1));
            var m = $"INSERTED|{c}|({(modeBulk.Equals(1) ? $"Bulk Insert PK RowId into {typeof(TEntity).Name}" : $"PK RowId into {typeof(TEntity).Name}")})";
            var e = Convert.ToInt64(c) < 1 ? nameof(HttpStatusCode.NotFound) : nameof(HttpStatusCode.OK);

            return new ApiResult(e, Convert.ToInt64(c), m);
        }
        catch (Exception ex)
        {
            return new ApiResult(nameof(HttpStatusCode.NotFound), Litterals.NoDataRow, ex.InnerException?.Message ?? ex.Message);
        }
    }

    [Produces(Litterals.ApplicationJson)]
    [HttpPost("[area]/[controller]/GetAllSqlRawAsync")]
    public IActionResult GetAllSqlRawAsync([FromBody] byte[] sqlRawBytes)
    {
        int count = 0;
        var sql = LZString.DecompressFromEncodedURIComponent(JsonSerializer.Deserialize<string>(sqlRawBytes));
        var listResult = _service.GetAllSqlRaw(sql, ref count);

        return count > -1
            ? Ok(listResult)
            : BadRequest(listResult);
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
    [ApiExplorerSettings(IgnoreApi = true)]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DataResult))]
    [HttpPost("[area]/[controller]/GetPagedQueryable")]
    public async Task<DataResult<TEntity>> GetPagedQueryableAsync([FromQuery] ODataQueryOptions<TEntity> queryOptions, [FromBody] DataManagerRequest dm)
    {
        //await StorageRequest.TestAsync();

        return await ValueTask.FromResult(_service.GetPagedQueryable(queryOptions, dm));
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [Produces(Litterals.ApplicationJson)]
    [HttpPost("[area]/[controller]/GetSelectExpandoFrom")]
    public async Task<IEnumerable<ExpandoObject>> GetSelectExpandoFromAsync([FromQuery] ODataQueryOptions<TEntity> queryOptions, [FromBody] string[] propertyNames)
        => await ValueTask.FromResult(_service.SelectExpandoFrom(queryOptions, propertyNames));

    /// <summary>
    /// [UPDATE]
    /// </summary>
    /// <param name="list"></param>
    /// <param name="modeBulk"></param>
    /// <returns>Odata</returns>
    [Produces(Litterals.ApplicationJson)]
    [HttpPut("[area]/[controller]/UpdateAsync")]
    public async Task<ApiResult> UpdateAsync([FromBody] byte[] list, [FromHeader(Name = "modebulk")] int modeBulk = 0)
    {
        try
        {
            var data = JsonSerializer.Deserialize<IList<TEntity>>(list);
            var c = await _service.UpdateAsync(data, modeBulk.Equals(1));
            var m = $"UPDATED|{c}|({(modeBulk.Equals(1) ? $"Bulk Update row(s) from {typeof(TEntity).Name}" : $"Row(s) from {typeof(TEntity).Name}")})";
            var e = c < 1 ? nameof(HttpStatusCode.NotFound) : nameof(HttpStatusCode.OK);

            return new ApiResult(e, c, m);
        }
        catch (Exception ex)
        {
            return new ApiResult(nameof(HttpStatusCode.NotFound), Litterals.NoDataRow, ex.InnerException?.Message ?? ex.Message);
        }
    }

    ///// <summary>
    ///// PATCH (DELTA UPDATE)
    ///// </summary>
    ///// <param name="id"></param>
    ///// <param name="newValues"></param>
    ///// <returns></returns>
    //[Produces(Litterals.ApplicationJson)]
    //[HttpPatch("[area]/[controller]")]
    //public async Task<IActionResult> Patch(string id, [FromBody] string newValues)
    //{
    //    try
    //    {
    //        var c = await _service.Patch(id, newValues);
    //        var m = $"PATCHED|{(c < 1 ? 0 : c)}|Row(s) from {typeof(TEntity).Name}";
    //        var e = c < 1 ? nameof(HttpStatusCode.NotFound) : nameof(HttpStatusCode.OK);

    //        return (c < 1)
    //            ? NotFound(value: OdataTools.OnODataSetValues(m, e))
    //            : Ok(value: OdataTools.OnODataSetValues(m, e));
    //    }
    //    catch (Exception ex)
    //    {
    //        return NotFound(value: OdataTools.OnODataError(ex));
    //    }
    //}

    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult))]
    [HttpPatch("[area]/[controller]/PatchAsync")]
    public async Task<ApiResult> PatchAsync([FromBody] PatchDoc patchDocs)
    {
        try
        {
            var c = await _service.PatchEx(patchDocs.EntityIds, patchDocs.PatchDocs);
            var m = $"PATCHED|{(c < 1 ? 0 : c)}|Row(s) from {typeof(TEntity).Name}";
            var e = c < 1 ? nameof(HttpStatusCode.NotFound) : nameof(HttpStatusCode.OK);

            return new ApiResult(e, c, m);
        }
        catch (Exception ex)
        {
            return new ApiResult(nameof(HttpStatusCode.NotFound), Litterals.NoDataRow, ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <summary>
    /// [DELETE]
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="modeBulk"></param>
    /// <returns>Odata</returns>
    [Produces(Litterals.ApplicationJson)]
    [HttpDelete("[area]/[controller]/DeleteAsync")]
    public async Task<ApiResult> DeleteAsync([FromBody] byte[] ids, [FromHeader(Name = "modebulk")] int modeBulk = 0)
    {
        try
        {
            var c = await _service.DeleteMany(ids, modeBulk.Equals(1));
            var m = $"DELETED|{(c < 0 ? 0 : c)}|Row(s) from {typeof(TEntity).Name}";
            var e = c < 1 ? nameof(HttpStatusCode.NotFound) : nameof(HttpStatusCode.OK);

            return new ApiResult(e, c, m);
        }
        catch (Exception ex)
        {
            return new ApiResult(nameof(HttpStatusCode.NotFound), Litterals.NoDataRow, ex.InnerException?.Message ?? ex.Message);
        }
    }

    #endregion CRUD ENDPOINTS
}

[Route("")]
public class GenericController<TEntity, TContext> : ODataController
    where TEntity : class
    where TContext : DbContext
{
    private readonly IGenericService<TEntity, TContext> _service;
    public GenericController(IGenericService<TEntity, TContext> service) => _service = service;

    [HttpPost("GetAllAsync")]
    [Produces(Litterals.ApplicationJson)]
    public async Task<PagedResponse<TEntity>> GetAllAsync([FromBody] DataManagerRequest dm) => await _service.GetAllAsync(dm);

    [HttpPost("UpdateAsync")]
    [Produces(Litterals.ApplicationJson)]
    public void UpdateAsync([FromBody] CRUDModel<TEntity> record) => _service.UpdateAsync(record);

    [HttpPost("AddAsync")]
    [Produces(Litterals.ApplicationJson)]
    public void AddAsync([FromBody] CRUDModel<TEntity> record) => _service.AddAsync(record);

    [HttpPost("RemoveAsync")]
    [Produces(Litterals.ApplicationJson)]
    public void RemoveAsync([FromBody] CRUDModel<TEntity> record) => _service.RemoveAsync(record);
}