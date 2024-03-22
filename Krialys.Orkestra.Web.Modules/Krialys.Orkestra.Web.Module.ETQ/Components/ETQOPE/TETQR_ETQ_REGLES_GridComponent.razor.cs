using Krialys.Data.EF.Etq;
using Krialys.Orkestra.Web.Module.Common.Components;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using static Krialys.Orkestra.Common.Shared.ETQ;
using Action = Syncfusion.Blazor.Grids.Action;

namespace Krialys.Orkestra.Web.Module.ETQ.Components.ETQOPE;

public partial class TETQR_ETQ_REGLES_GridComponent
{
    #region Parameters

    /// <summary>
    /// Specifies query to select data from DataSource.
    /// </summary>
    [Parameter] public Query GridQuery { get; set; }

    /// <summary>
    /// List of the selected objects in the component.
    /// </summary>
    [Parameter] public int EtiquetteId { get; set; }

    /// <summary>
    /// Objet associé à Etiquette. Requis pour la gestion des changements des valeurs de regles
    /// </summary>
    [Parameter] public int ObjetEtiquetteId { get; set; }

    /// <summary>
    /// Is the user allowed to modify grid data ?
    /// </summary>
    [Parameter] public bool AllowModify { get; set; }
    #endregion    

    private string UserId => Session.GetUserId();

    private OrkaGenericGridComponent<TETQR_ETQ_REGLES> Ref_TETQR_ETQ_REGLES;

    // Keep RegleValeurSelected public for bind
    public int RegleValeurSelected { get; set; }//= null;
    public string UserComment { get; set; }//= null;

    //private bool IsEdit { get; set; } = false;

    public string GetHeader(TETQR_ETQ_REGLES value)
    {
        return "Editer la règle"; // c'est quoi cette daube??
    }

    // liste des regles valeurs non filtrée
    private IEnumerable<TRGLRV_REGLES_VALEURS> _listeRegleValeurEtq = Enumerable.Empty<TRGLRV_REGLES_VALEURS>();

    //liste des regles valeurs choisissables pour la regle sélectionnée par l'utilisateur
    public IEnumerable<TRGLRV_REGLES_VALEURS> ListeRegleValeurfiltre { get; set; } = Enumerable.Empty<TRGLRV_REGLES_VALEURS>();

    /* on   propose les valeurs associées à la regle excepté la valeur d origine */
    public void rgvaleur(int regleId, int reglevaleurIDaEcarter)
    {
        // Highly recommended because it meets the expectations of the target => "DataSource" is expecting IEnumerable<TRGLRV_REGLES_VALEURS>, nothing else!
        ListeRegleValeurfiltre = _listeRegleValeurEtq
            .Where(s => s.TRGL_REGLEID.Equals(regleId) && !s.TRGLRV_REGLES_VALEURID.Equals(reglevaleurIDaEcarter))
            .OrderBy(x => x.TRGLRV_ORDRE_AFFICHAGE)
            .AsQueryable()   // => works as well since a Queryable is also Enumerable
            .AsEnumerable(); // => not mandatory here since Queryable has already made the covariance
    }

    protected override async Task OnInitializedAsync()
        => _listeRegleValeurEtq = await ProxyCore.GetEnumerableAsync<TRGLRV_REGLES_VALEURS>(useCache: false);

    private void OnActionBegin(ActionEventArgs<TETQR_ETQ_REGLES> args)
    {
        if (args.RequestType is Action.BeforeBeginEdit or
             Action.BeginEdit)
        {
            //IsEdit = true;
            RegleValeurSelected = -1;
            UserComment = "";
        }
    }

    /// <summary>
    /// Custom event to handle and manipulate datas before saving to database
    /// Some business rules can be applied here using EndEdit, or reject globally using CloseEdit()
    /// </summary>
    /// <param name="instance">Parent OrkaGenericGridComponent instance</param>
    /// <param name="entity">Incoming datas to be saved</param>
    /// <returns></returns>
    private async Task SaveAsync<TEntity>(OrkaGenericGridComponent<TEntity> instance, object entity) where TEntity : class, new()
    {
        if (entity is TETQR_ETQ_REGLES tetqrEtqRegle)
        {
            if (RegleValeurSelected > 0)
            {
                try
                {
                    var etqRules = new List<EtqRules>
                    {
                        new EtqRules
                        {
                            ActeurId = UserId,
                            TETQ_ETIQUETTEID = EtiquetteId,
                            TETQR_ETQ_REGLEID = tetqrEtqRegle.TETQR_ETQ_REGLEID,
                            TRGLRV_REGLES_VALEURID_PARENT = tetqrEtqRegle.TRGLRV_REGLES_VALEURID,
                            TRGLRV_REGLES_VALEURID = RegleValeurSelected,
                            TOBJE_OBJET_ETIQUETTEID = ObjetEtiquetteId,
                            COMMENT = UserComment
                        }
                    };

                    await ProxyCore.EtqApplyRules(etqRules);

                    // on ne fait pas de sauvegarde via cette fenetre , c est la methode ApplyRulesEtq qui s en charge !!!
                    // il  faut d ailleurs NE surtout PAS sauvegarder ce qu'il y a sur l'IHM
                    await instance.DataGrid.CloseEditAsync();
                    await RefreshGridDataAsync();
                }
                catch (Exception ex)
                {
                    await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], ex.Message);
                }
            }
        }
    }

    private Task RefreshGridDataAsync()
    {
        // Clear caches.
        ProxyCore.CacheRemoveEntities(typeof(TETQR_ETQ_REGLES), typeof(TSEQ_SUIVI_EVENEMENT_ETQS));

        // Refresh grid data.
        return Ref_TETQR_ETQ_REGLES.DataGrid.Refresh();
    }
}