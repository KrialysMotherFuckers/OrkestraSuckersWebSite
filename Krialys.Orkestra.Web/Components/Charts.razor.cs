using Krialys.Common.Literals;
using Krialys.Data.EF.Etq;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.DI;
using Krialys.Orkestra.Web.Module.Common.Models;
using Krialys.Orkestra.WebApi.Proxy;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace Krialys.Orkestra.Web.Components;

public partial class Charts : ComponentBase
{
    [Inject] private IHttpProxyCore Proxy { get; set; }
    [Inject] private IUserSessionStatus UserSession { get; set; }
    [Inject] private NavigationManager NavManager { get; set; }

    // Data Etiquettes (jeux de données produits (en nombre))
    private IEnumerable<VACCGET_ACCUEIL_GRAPHE_ETQS> _dataEtiquettes = Enumerable.Empty<VACCGET_ACCUEIL_GRAPHE_ETQS>();
    // Data Demandes (Productions (en nombre))
    private IEnumerable<VACCGD_ACCUEIL_GRAPHE_DEMANDES> _dataDemandes = Enumerable.Empty<VACCGD_ACCUEIL_GRAPHE_DEMANDES>();
    // Data quality (Qualité des données (en pourcentage))
    private IEnumerable<VACCGQ_ACCUEIL_GRAPHE_QUALITES> _dataQuality = Enumerable.Empty<VACCGQ_ACCUEIL_GRAPHE_QUALITES>();

    /// <summary>
    /// Get Culture
    /// </summary>
    private string Culture
        => CultureInfo.CurrentCulture.Name[..2].ToUpper().Equals(CultureLiterals.French, StringComparison.Ordinal)
        ? CultureLiterals.French
        : CultureLiterals.English;

    protected override async Task OnInitializedAsync()
    {
        if (UserSession.IsConnected)
        {
            _dataEtiquettes = await Proxy.GetEnumerableAsync<VACCGET_ACCUEIL_GRAPHE_ETQS>("?$orderby=PERIODE", useCache: true);
            _dataDemandes = await Proxy.GetEnumerableAsync<VACCGD_ACCUEIL_GRAPHE_DEMANDES>("?$orderby=PERIODE", useCache: true);
            _dataQuality = await Proxy.GetEnumerableAsync<VACCGQ_ACCUEIL_GRAPHE_QUALITES>("?$orderby=QUALIFID", useCache: true);
        }

        await base.OnInitializedAsync();
    }
}
