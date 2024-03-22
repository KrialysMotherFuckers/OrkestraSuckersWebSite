using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Syncfusion.Blazor.Data;

namespace Krialys.Orkestra.Web.Module.ADM.Components.DTFAdmin;

public partial class DataDrivenDTFProfilCopyComponent
{
    #region Constant


    /*  
     *  TRCLI_CLIENTAPPLICATIONID 7 DTF
    *   TRCL_CLAIMID 4 "datadriven role
    */
    private const int DTF = 7; //"DTF" 
    private const string ClaimvalueDataDriven = "4"; //datadriven role
    #endregion

    #region Parameters





    /// <summary>
    /// Sort on user name
    /// </summary>                                          
    private readonly Query UserQuery = new Query()
   .Sort(nameof(TRU_USERS.TRU_FULLNAME), "Ascending");


    #endregion

    private string UserId { get; set; }   // id user courant

    protected override async Task OnInitializedAsync()
    {
        UserId = Session.GetUserId();

        //si on vient affecter des droits juste apres la creation d'utilisateurs on a besoin d'une liste utilisateur a jour
        var users_QueryOption = $"?$expand=TRUCL_USERS_CLAIMS($filter=TRUCL_STATUS eq '{StatusLiteral.Available}' and TRCLI_CLIENTAPPLICATIONID eq {DTF} and TRUCL_CLAIM_VALUE eq '{ClaimvalueDataDriven}' )&$filter=TRU_STATUS eq 'A' and TRUCL_USERS_CLAIMS/any(TRS: TRS/TRUCL_CLAIM_VALUE  eq '{ClaimvalueDataDriven}')";
        _users = await ProxyCore.GetEnumerableAsync<TRU_USERS>(users_QueryOption, useCache: false);
        //http://localhost:8000/api/univers/v1/TRU_USERS?$expand=TRUCL_USERS_CLAIMS($filter=TRUCL_STATUS eq '{StatusLiteral.Available}' and TRCLI_CLIENTAPPLICATIONID eq 7 and TRUCL_CLAIM_VALUE eq '4' )&$filter=TRU_STATUS eq 'A' and TRUCL_USERS_CLAIMS/any(TRS: TRS/TRUCL_CLAIM_VALUE  eq '4')
    }

    #region User actif column
    /// <summary>
    /// </summary>
    private IEnumerable<TRU_USERS> _users = Enumerable.Empty<TRU_USERS>();


    private string UserSrc = "";
    private string UserDest = "";

    /// <summary>
    /// List only TRU_USERS with "Active" status & ordered. 
    /// </summary>
    private IEnumerable<TRU_USERS> ActiveUsers
    {
        get
        {
            return _users;  // a trier sur TRU_FULLNAME mais how ?
        }
    }



    #endregion

    private async Task SaveAsync()
    {
        if (UserSrc == null || UserDest == null)
            return;

        if (UserDest.Length != 0 && UserSrc.Length != 0)
        {
            //anti con
            if (UserDest == UserSrc)
            {
                await Toast.DisplayWarningAsync(Trad.Keys["COMMON:Error"], Trad.Keys["Administration:DataDrivenDTFCopyBadChoice"]);
                return;
            }

            // etat master actif
            //etat Actif  ou Proto (A/P)
            //module Actif (A)
            //habilitation active

            string queryOptions = $"?$expand=TS_SCENARIO($expand=TE_ETAT($expand=TEM_ETAT_MASTER))&$filter=TRU_USERID eq '{UserSrc}' and (TS_SCENARIO/TE_ETAT/TRST_STATUTID eq '{StatusLiteral.Prototype}' or  TS_SCENARIO/TE_ETAT/TRST_STATUTID eq '{StatusLiteral.Available}') and TS_SCENARIO/TRST_STATUTID eq '{StatusLiteral.Available}' and TS_SCENARIO/TE_ETAT/TEM_ETAT_MASTER/TRST_STATUTID eq '{StatusLiteral.Available}' and TRST_STATUTID eq 'A'";

            // http://localhost:8000/api/univers/v1/TH_HABILITATIONS?$expand=TS_SCENARIO($expand=TE_ETAT($expand=TEM_ETAT_MASTER))&$filter=TRU_USERID eq '3' and (TS_SCENARIO/TE_ETAT/TRST_STATUTID eq '{StatusLiteral.Prototype}' or  TS_SCENARIO/TE_ETAT/TRST_STATUTID eq '{StatusLiteral.Available}') and TS_SCENARIO/TRST_STATUTID eq '{StatusLiteral.Available}' and TS_SCENARIO/TE_ETAT/TEM_ETAT_MASTER/TRST_STATUTID eq '{StatusLiteral.Available}' and TRST_STATUTID eq '{StatusLiteral.Available}'
            var habSrc = await ProxyCore.GetEnumerableAsync<TH_HABILITATIONS>(queryOptions, useCache: false);
            IList<TH_HABILITATIONS> itemsToCreate = new List<TH_HABILITATIONS>();
            foreach (var Module in habSrc)
            {

                TH_HABILITATIONS newItem = new()
                {
                    TRU_USERID = UserDest,//habilitation.TRU_USERID,
                    TS_SCENARIOID = Module.TS_SCENARIOID,
                    TH_COMMENTAIRE = Module.TH_COMMENTAIRE,
                    TH_DROIT_CONCERNE = Module.TH_DROIT_CONCERNE,

                    TH_DATE_INITIALISATION = DateExtensions.GetLocaleNow(),
                    TRU_INITIALISATION_AUTEURID = UserId,
                    TH_MAJ_DATE = DateExtensions.GetLocaleNow(),
                    TRU_MAJ_AUTEURID = UserId,
                    TH_EST_HABILITE = Module.TH_EST_HABILITE,
                    TRST_STATUTID = Module.TRST_STATUTID
                    //champs non exploité liés aux groupes
                    //TTE_TEAMID
                    //TSG_SCENARIO_GPEID
                    //TH_HERITE_HABILITATIONID
                };

                // Add this item to the list of elements to create.
                itemsToCreate.Add(newItem);
            }

            if (itemsToCreate.Any())
            {
                // Add these items to the table.
                var apiResult = await ProxyCore.InsertAsync(itemsToCreate);
                // If the insertion failed.
                if (apiResult.Count == -1)
                {
                    await ProxyCore.SetLogException(new LogException(GetType(), itemsToCreate, apiResult.Message));
                    await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"] + apiResult.Message);
                    return;
                }

                await Toast.DisplaySuccessAsync(Trad.Keys["COMMON:Succes"], itemsToCreate.Count + " " + Trad.Keys["COMMON:RecordDone"]);
                ProxyCore.CacheRemoveEntities(typeof(TH_HABILITATIONS));
            }
            else
            {
                // nothing to add
                await Toast.DisplaySuccessAsync(Trad.Keys["COMMON:Succes"], "0" + " " + Trad.Keys["COMMON:RecordDone"]);

            }
        }
    }
}