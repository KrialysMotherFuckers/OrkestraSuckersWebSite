using Krialys.Common.Extensions;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Krialys.Common.Literals;
using Microsoft.AspNetCore.Components;

namespace Krialys.Orkestra.Web.Module.DTM.Pages.Settings.References.Components;

public partial class TC_CATEGORIES_GridComponent
{
    #region Parameters
    /// <summary>
    /// Is the user allowed to modify grid data ?
    /// </summary>
    [Parameter] public bool AllowModify { get; set; } = true;
    #endregion

    private OrkaGenericGridComponent<TC_CATEGORIES> Ref_TC_CATEGORIES;

    /// <summary>
    /// Custom event to handle and manipulate datas before saving to database
    /// Some business rules can be applied here using EndEdit, or reject globally using CloseEdit()
    /// </summary>
    /// <param name="instance">Parent OrkaGenericGridComponent instance</param>
    /// <param name="entity">Incoming datas to be saved</param>
    /// <returns></returns>
    private async Task SaveAsync<TEntity>(OrkaGenericGridComponent<TEntity> instance, object entity) where TEntity : class, new()
    {
        if (entity is TC_CATEGORIES tcCategorie)
        {
            tcCategorie.TC_NOM = tcCategorie.TC_NOM.FirstCharToUpper();

            if (tcCategorie!.TC_CATEGORIEID == 0)
            {
                tcCategorie.TC_DATE_CREATION = DateExtensions.GetLocaleNow();
                tcCategorie.TRST_STATUTID = StatusLiteral.Available;
            }
        }

        await instance.DataGrid.EndEditAsync();
    }
}
