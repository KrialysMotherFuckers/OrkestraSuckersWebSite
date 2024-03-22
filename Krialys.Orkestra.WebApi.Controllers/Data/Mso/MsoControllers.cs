using Krialys.Data.EF.Mso;
using Krialys.Orkestra.WebApi.Controllers.Common;
using Krialys.Orkestra.WebApi.Services.Common;

// > Implements GenericCRUDController for TRA_ATTENDUS
namespace Krialys.Orkestra.WebApi.Controllers.MSO;

/// <summary>
/// This class represents the route to be applied
/// </summary>
internal static class Route { internal const string Name = Litterals.MsoRootPath; }

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRA_ATTENDUSController : GenericCrudController<TRA_ATTENDUS, KrialysDbContext>
{
    public TRA_ATTENDUSController(GenericCrud<TRA_ATTENDUS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRAP_ATTENDUS_PLANIFSController : GenericCrudController<TRAP_ATTENDUS_PLANIFS, KrialysDbContext>
{
    public TRAP_ATTENDUS_PLANIFSController(GenericCrud<TRAP_ATTENDUS_PLANIFS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRAPL_APPLICATIONSController : GenericCrudController<TRAPL_APPLICATIONS, KrialysDbContext>
{
    public TRAPL_APPLICATIONSController(GenericCrud<TRAPL_APPLICATIONS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRC_CONTRATSController : GenericCrudController<TRC_CONTRATS, KrialysDbContext>
{
    public TRC_CONTRATSController(GenericCrud<TRC_CONTRATS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRC_CRITICITESController : GenericCrudController<TRC_CRITICITES, KrialysDbContext>
{
    public TRC_CRITICITESController(GenericCrud<TRC_CRITICITES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRNF_NATURES_FLUXController : GenericCrudController<TRNF_NATURES_FLUX, KrialysDbContext>
{
    public TRNF_NATURES_FLUXController(GenericCrud<TRNF_NATURES_FLUX, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRNT_NATURES_TRAITEMENTSController : GenericCrudController<TRNT_NATURES_TRAITEMENTS, KrialysDbContext>
{
    public TRNT_NATURES_TRAITEMENTSController(GenericCrud<TRNT_NATURES_TRAITEMENTS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRP_PLANIFSController : GenericCrudController<TRP_PLANIFS, KrialysDbContext>
{
    public TRP_PLANIFSController(GenericCrud<TRP_PLANIFS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRR_RESULTATSController : GenericCrudController<TRR_RESULTATS, KrialysDbContext>
{
    public TRR_RESULTATSController(GenericCrud<TRR_RESULTATS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRTT_TECHNOS_TRAITEMENTSController : GenericCrudController<TRTT_TECHNOS_TRAITEMENTS, KrialysDbContext>
{
    public TRTT_TECHNOS_TRAITEMENTSController(GenericCrud<TRTT_TECHNOS_TRAITEMENTS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TTL_LOGSController : GenericCrudController<TTL_LOGS, KrialysDbContext>
{
    public TTL_LOGSController(GenericCrud<TTL_LOGS, KrialysDbContext> genericService) : base(genericService) { }
}