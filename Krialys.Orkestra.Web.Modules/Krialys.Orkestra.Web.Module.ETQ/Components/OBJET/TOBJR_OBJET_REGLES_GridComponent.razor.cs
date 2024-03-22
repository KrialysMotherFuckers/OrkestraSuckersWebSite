using Krialys.Data.EF.Etq;
using Krialys.Orkestra.Web.Module.Common.Components;
using Krialys.Common.Literals;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Action = Syncfusion.Blazor.Grids.Action;

namespace Krialys.Orkestra.Web.Module.ETQ.Components.OBJET;

public partial class TOBJR_OBJET_REGLES_GridComponent
{
    #region Parameters

    /// <summary>
    /// Specifies query to select data from DataSource.
    /// </summary>
    [Parameter] public Query GridQuery { get; set; }

    /// <summary>
    /// List of the selected objects in the component.
    /// </summary>
    [Parameter] public int ObjetEtiquetteId { get; set; }

    [Parameter] public int? StatutObjet { get; set; }

    /// <summary>
    /// Is the user allowed to modify grid data ?
    /// </summary>
    [Parameter] public bool AllowModify { get; set; }
    #endregion

    private OrkaGenericGridComponent<TOBJR_OBJET_REGLES> _refTobjrObjetRegles;

    private string CheckedValueStatut { get; set; } = "Unchanged";  // choix du nouveau statut (inchangé ou pas)

    public string GetHeader(TOBJR_OBJET_REGLES value)
    {
        return "Editer la règle";
    }

    private void OnActionBegin(ActionEventArgs<TOBJR_OBJET_REGLES> args)
    {
        args.PreventRender = true;

        if (args.RequestType is Action.BeginEdit or Action.Add)
        {
            // IsEditOrAdd = true;
            CheckedValueStatut = "Unchanged"; // reinitialise au cas ou on etait sur un autre enregistrement auparavant
        }
    }

    /// <summary>
    /// Get change on regle rules.
    /// </summary>
    private void SfRadioButtonStatutOnChange(ChangeEventArgs args)
    {
        CheckedValueStatut = (string)args.Value;
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
        if (entity is TOBJR_OBJET_REGLES tobjrObjetRegle)
        {
            var tobjrObjetRegleId = tobjrObjetRegle.TOBJR_OBJET_REGLEID;

            if (tobjrObjetRegleId != 0)  // ==>On est sur un EDIT d'un enregistrement qu'on modifie 
            {
                switch (CheckedValueStatut)
                {
                    case "Unchanged":
                        break;

                    case StatusLiteral.Yes:
                        tobjrObjetRegle.TOBJR_APPLICABLE = CheckedValueStatut;
                        break;

                    case StatusLiteral.No:
                        tobjrObjetRegle.TOBJR_APPLICABLE = CheckedValueStatut;
                        break;
                }
            }

            await instance.DataGrid.EndEditAsync();
        }
    }
}