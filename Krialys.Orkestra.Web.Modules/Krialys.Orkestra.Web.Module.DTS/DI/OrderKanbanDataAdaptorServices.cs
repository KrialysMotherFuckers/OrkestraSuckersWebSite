using Krialys.Data.EF.Univers;
using Krialys.Orkestra.WebApi.Proxy;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;

namespace Krialys.Orkestra.Web.Module.DTS.DI;

public interface IOrderKanbanDataAdaptorServices
{
    Task<object> ReadAsync(DataManagerRequest dataManagerRequest, string key = null);

    Task<object> InsertAsync(DataManager dataManager, object data, string key);

    Task<object> RemoveAsync(DataManager dataManager, object data, string keyField, string key);

    Task<object> UpdateAsync(DataManager dataManager, object data, string keyField, string key);
}

public class OrderKanbanDataAdaptorServices : DataAdaptor, IOrderKanbanDataAdaptorServices
{
    #region Injected services
    /// <summary>
    /// Service used to communicate with the proxy.
    /// </summary>
    private readonly IHttpProxyCore _proxyCore;

    /// <summary>
    /// Service used to manage requests related to orders (TCMD_COMMANDES).
    /// </summary>
    private readonly IOrderManagementServices _orderManagement;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderKanbanDataAdaptorServices"/> class.
    /// </summary>
    /// <param name="proxyCore">Service used to communicate with the proxy.</param>
    /// <param name="orderManagement">Service used to manage requests related to orders (TCMD_COMMANDES).</param>
    public OrderKanbanDataAdaptorServices(IHttpProxyCore proxyCore,
        IOrderManagementServices orderManagement)
    {
        _proxyCore = proxyCore;
        _orderManagement = orderManagement;
    }
    #endregion

    #region Properties
    /// <summary>
    /// Orders read from database.
    /// </summary>
    IEnumerable<TCMD_COMMANDES> _orders;
    #endregion

    #region Read
    public override async Task<object> ReadAsync(DataManagerRequest dataManagerRequest, string key = null)
    {
        // Odata query filter: select only phases displayed in Kanban.
        string filterQuery =
            $"{nameof(TCMD_COMMANDES.TCMD_PH_PHASE)}/{nameof(TCMD_PH_PHASES.TCMD_PH_CODE)} " +
            $"in ('{Phases.ToAccept}','{Phases.InProgress}','{Phases.Frost}','{Phases.Delivered}','{Phases.Rejected}')";

        // Odata query applied on TCMD_COMMANDES table:
        //  - Expand on TCMD_PH_PHASES table,
        //  - Expand on TCMD_MC_MODE_CREATIONS table,
        //  - Expand on TE_ETATS table,
        //  - Expand on TCMD_DOC_DOCUMENTS table,
        //  - Filter.
        string query = $"?$expand={nameof(TCMD_COMMANDES.TCMD_PH_PHASE)}," +
            $"{nameof(TCMD_COMMANDES.TCMD_MC_MODE_CREATION)}," +
            $"{nameof(TCMD_COMMANDES.TE_ETAT)}," +
            $"{nameof(TCMD_COMMANDES.TCMD_DOC_DOCUMENTS)}" +
            $"&$filter={filterQuery}";

        // Read orders.
        _orders = await _proxyCore.GetEnumerableAsync<TCMD_COMMANDES>(query);

        // Return orders (and count if needed).
        var tcmdCommandeses = _orders as TCMD_COMMANDES[] ?? _orders.ToArray();

        return dataManagerRequest.RequiresCounts ?
            new DataResult { Result = tcmdCommandeses, Count = tcmdCommandeses.Count() } : _orders;
    }
    #endregion

    #region Insert
    public override async Task<object> InsertAsync(DataManager dataManager,
        object data, string key)
    {
        await Task.Yield();

        // Insertion is not supported.
        return Enumerable.Empty<TCMD_COMMANDES>();
    }
    #endregion

    #region Remove
    public override Task<object> RemoveAsync(DataManager dataManager,
        object data, string keyField, string key)
    {
        // Removal is not supported.
        throw new NotImplementedException();
    }
    #endregion

    #region Update
    public override object Update(DataManager dataManager,
        object data, string keyField, string key)
    {
        // Try to cast data to write (new data).
        if (data is TCMD_COMMANDES newData)
        {
            // Get previous data (old data).
            var oldData = _orders
                .FirstOrDefault(o => o.TCMD_COMMANDEID.Equals(newData.TCMD_COMMANDEID));

            // If the new phase is different of the old phase.
            if (oldData?.TCMD_PH_PHASE?.TCMD_PH_CODE is not null &&
                !oldData.TCMD_PH_PHASE.TCMD_PH_CODE.Equals(newData.TCMD_PH_PHASE.TCMD_PH_CODE))
            {
                // Change phase depending on the new phase value.
                switch (newData.TCMD_PH_PHASE.TCMD_PH_CODE)
                {
                    case Phases.InProgress:
                        // Open edit dialog so user can fill in the required fields
                        // before changing phases.
                        _orderManagement.OnOpenEditDialog(newData);
                        break;

                    case Phases.Frost:
                        // Change order to phase Frost.
                        _orderManagement.FrostOrder(newData);
                        break;

                    case Phases.Delivered:
                        // Open edit dialog so user can fill in the required fields
                        // before changing phases.
                        _orderManagement.OnOpenEditDialog(newData);
                        break;

                    case Phases.Rejected:
                        // Change order to phase Rejected.
                        _orderManagement.RejectOrder(newData);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        return data;
    }
    #endregion
}
