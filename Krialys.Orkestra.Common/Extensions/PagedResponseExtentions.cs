using Krialys.Orkestra.Common.Models;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using System.Collections;

namespace Krialys.Orkestra.Common.Extensions;

public static class PagedResponseExtentions
{
    public static PagedResponse<T> GetPagedResponse<T>(this IEnumerable dataSource, DataManagerRequest dm) where T : class
    {
        if (dm.Search != null && dm.Search.Count > 0) dataSource = DataOperations.PerformSearching(dataSource, dm.Search);  //Search
        if (dm.Sorted != null && dm.Sorted.Count > 0) dataSource = DataOperations.PerformSorting(dataSource, dm.Sorted);
        if (dm.Where != null && dm.Where.Count > 0) dataSource = DataOperations.PerformFiltering(dataSource, dm.Where, dm.Where[0].Operator);

        int recordCount = dataSource.Cast<T>().Count();

        if (dm.Skip != 0) dataSource = DataOperations.PerformSkip(dataSource, dm.Skip);
        if (dm.Take != 0) dataSource = DataOperations.PerformTake(dataSource, dm.Take);

        return
            new PagedResponse<T>()
            {
                result = dataSource.Cast<T>(),
                count = recordCount
            };
    }

    public static IQueryable<T> Execute<T>(this IQueryable<T> dataSource, DataManagerRequest manager)
    {
        if (manager == null) return dataSource;

        if (manager.Where != null && manager.Where.Count() > 0) dataSource = QueryableOperation.PerformFiltering<T>(dataSource, manager.Where, string.Empty);
        if (manager.Search != null && manager.Search.Count() > 0) dataSource = QueryableOperation.PerformSearching<T>(dataSource, manager.Search);
        if (manager.Sorted != null && manager.Sorted.Count() > 0) dataSource = QueryableOperation.PerformSorting<T>(dataSource, manager.Sorted);
        if (manager.Skip != 0) dataSource = QueryableOperation.PerformSkip<T>(dataSource, manager.Skip);
        if (manager.Take != 0) dataSource = QueryableOperation.PerformTake<T>(dataSource, manager.Take);

        return dataSource;
    }
}
