using Krialys.Common.Literals;
using Krialys.Data.EF.Etq;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common;
using Krialys.Orkestra.WebApi.Services.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using static Krialys.Orkestra.Common.Shared.ETQ;
using KrialysDbContext = Krialys.Data.EF.Univers.KrialysDbContext;

namespace Krialys.Orkestra.WebApi.Controllers.Data.Etq;

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Litterals.EtqRootPath)]
[Route("[area]/[controller]")]
[ApiController]
public class EtiquetteDetailsController : ControllerBase
{
    private readonly KrialysDbContext _dbContext;
    private readonly Krialys.Data.EF.Etq.KrialysDbContext _dbContextEtq;
    private readonly ICommonServices _commonServices;

    public EtiquetteDetailsController(KrialysDbContext dbContext, Krialys.Data.EF.Etq.KrialysDbContext dbContextEtq,
        ICommonServices commonServices)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbContextEtq = dbContextEtq ?? throw new ArgumentNullException(nameof(dbContextEtq));
        _commonServices = commonServices;
    }

    /// <summary>
    /// Get EtiquetteDetails, join between TETQ_ETIQUETTES and VDE_DEMANDES_ETENDUES.
    /// </summary>
    /// <param name="queryOptions">Odata query applied on data.</param>
    /// <param name="pageSize">Number of items in page.</param>
    /// <returns>ODataContract : Result: One pageSize of EtiquetteDetails, Count: TETQ_ETIQUETTES with Odata query applied but without pageSize. </returns>
    [Authorize]
    [Produces(Litterals.ApplicationJson)]
    [HttpPost("GetEtiquetteDetails")]
    public ODataContract<EtiquetteDetails> GetEtiquetteDetails([FromBody] ODataQueryOptions<TETQ_ETIQUETTES> queryOptions, [FromHeader] int pageSize)
    {
        // Get user Id from claims.
        (string userId, _) = _commonServices.GetUserIdAndName();

        // Get VDE_DEMANDES_ETENDUES
        var demandesDbSet = _dbContext.Set<VDE_DEMANDES_ETENDUES>().AsNoTracking();

        // Get 'visible' labels TETQ_ETIQUETTES with allowed access for DTS:
        //  - Perimeter must allow DTS access (TPRCP_PRC_PERIMETRES.TPRCP_ALLOW_DTS_ACCESS = true),
        //  - Label must be public (TETQ_ETIQUETTES.etq_is_public_access = true),
        //  - Or if label is private, user must have an active label authorization (ETQ_TM_AET_Authorization).
        var etiquettesQueryable = _dbContextEtq.TETQ_ETIQUETTES.AsNoTracking()
            .Where(etq => etq.TPRCP_PRC_PERIMETRE.TPRCP_ALLOW_DTS_ACCESS.Equals(true) &&
                (etq.etq_is_public_access.Equals(true) ||
                etq.ETQ_TM_AET_Authorization.Any(aet => aet.aet_user_id.Equals(userId) && aet.aet_status_id.Equals(StatusLiteral.Available))
            ));

        // Apply odata queries on TETQ_ETIQUETTES and return IEnumerable.
        var etiquettesWithoutPagingEnum = queryOptions.ApplyTo(etiquettesQueryable).Cast<TETQ_ETIQUETTES>();

        var etiquettesEnum = queryOptions.ApplyTo(etiquettesQueryable, new ODataQuerySettings
        {
            PageSize = pageSize,
            TimeZone = TimeZoneInfo.Utc
        }).Cast<TETQ_ETIQUETTES>().AsEnumerable();

        // Left join between etiquettes and demandes
        // If demande doesn't exist, demande is null
        var result =
            from etiquette in etiquettesEnum
            join demande in demandesDbSet on etiquette.DEMANDEID equals demande.TD_DEMANDEID into groupJoin
            from demandeOrNull in groupJoin.DefaultIfEmpty()
            select new EtiquetteDetails
            {
                TETQ_ETIQUETTEID = etiquette.TETQ_ETIQUETTEID,
                TETQ_CODE = etiquette.TETQ_CODE,
                TETQ_LIB = etiquette.TETQ_LIB,
                TETQ_DATE_CREATION = etiquette.TETQ_DATE_CREATION,
                DEMANDEUR = demandeOrNull?.DEMANDEUR,
                REFERENT = demandeOrNull?.REFERENT,
            };

        var etiquetteDetailsEnumerable = result as EtiquetteDetails[] ?? result.ToArray();

        return new ODataContract<EtiquetteDetails>
        {
            // One pageSize of EtiquetteDetails.
            Result = etiquetteDetailsEnumerable.Any() ? etiquetteDetailsEnumerable : Enumerable.Empty<EtiquetteDetails>(),
            // Count of TETQ_ETIQUETTES with Odata query applied but without pageSize.
            Count = etiquettesWithoutPagingEnum.Count()
        };
    }
}
