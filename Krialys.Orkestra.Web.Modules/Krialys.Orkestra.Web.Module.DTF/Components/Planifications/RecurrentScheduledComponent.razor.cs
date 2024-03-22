using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Krialys.Orkestra.Web.Module.Common.PolicyBuilders;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;

namespace Krialys.Orkestra.Web.Module.DTF.Components.Planifications;

public partial class RecurrentScheduledComponent
{
    #region Datagrid
    private OrkaGenericGridComponent<VPE_PLANIF_ENTETES> Ref_Grid;

    //private IEnumerable<VPE_PLANIF_ENTETES> Data4Grid;//= Enumerable.Empty<VPE_PLANIF_ENTETES>(); // l initialisation sur empty entraine une erreur actionFailure qd le cache est a false
    readonly Query GridQuery = new Query();
    #endregion

    private bool isDataDriven;
    private bool isDTFAdm;
    private bool isDTFConsul;

    bool AllowInteract; // interaction possible ou pas avec boutons selon les droits 

    protected override async Task OnInitializedAsync()
    {
        isDataDriven = await Session.VerifyPolicy(PoliciesLiterals.DTFDataDriven);
        isDTFAdm = await Session.VerifyPolicy(PoliciesLiterals.DTFAdm);
        isDTFConsul = await Session.VerifyPolicy(PoliciesLiterals.DTFConsul);

        string queryOptions;

        if (isDataDriven)
        {
            // le double filtre sur TRU_USERID
            // le 1er contribue  a ne ramener que les enregistrements de VDTFH_HABILITATION avec userID  =x
            // le 2nd pour permettre de ramener que les enregistrements avec userid pour lesquels on a au moins userID  =x
            // les 2 sont requis en tout cas

            queryOptions = $"?$expand=TS_SCENARIO($expand=VDTFH_HABILITATION($filter=TRU_USERID eq '{Session.GetUserId()}' and (PRODUCTEUR eq 1 or CONTROLEUR eq 1)))&$filter=TS_SCENARIO/VDTFH_HABILITATION/any(o: o/TRU_USERID eq '{Session.GetUserId()}' and (o/PRODUCTEUR eq 1 or  o/CONTROLEUR eq 1))";
            //    http://localhost:8000/api/univers/v1/VPE_PLANIF_ENTETES?$expand=TS_SCENARIO($expand=VDTFH_HABILITATION($filter=TRU_USERID eq '3' and (PRODUCTEUR eq 1 or CONTROLEUR eq 1)))&$filter=TS_SCENARIO/VDTFH_HABILITATION/any(o: o/TRU_USERID eq '3' and (o/PRODUCTEUR eq 1 or  o/CONTROLEUR eq 1 ))
        }
        else queryOptions = isDTFAdm || isDTFConsul ? null : "?$filter=TS_SCENARIOID eq 0";

        GridQuery.AddParams(Litterals.OdataQueryParameters, queryOptions);
    }

    public void OnRowDataBound(RowDataBoundEventArgs<VPE_PLANIF_ENTETES> args)
    {
        // habilité au niveau data en producteur ou admin
        AllowInteract = (isDataDriven && args.Data?.TS_SCENARIO?.VDTFH_HABILITATION?.FirstOrDefault()?.PRODUCTEUR == 1) || isDTFAdm;
    }
}
