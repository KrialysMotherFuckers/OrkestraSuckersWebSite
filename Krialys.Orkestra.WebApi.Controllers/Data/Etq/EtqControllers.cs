using Krialys.Data.EF.Etq;
using Krialys.Orkestra.WebApi.Controllers.Common;
using Krialys.Orkestra.WebApi.Services.Common;

// > Implements GenericCRUDController for ETQ
namespace Krialys.Orkestra.WebApi.Controllers.Data.ETQ;


/// <summary>
/// This class represents the route to be applied
/// </summary>
internal static class Route { internal const string Name = Litterals.EtqRootPath; }

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TEQC_ETQ_CODIFSController : GenericCrudController<TEQC_ETQ_CODIFS, KrialysDbContext>
{
    public TEQC_ETQ_CODIFSController(GenericCrud<TEQC_ETQ_CODIFS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TTE_TYPE_EVENEMENTSController : GenericCrudController<TTE_TYPE_EVENEMENTS, KrialysDbContext>
{
    public TTE_TYPE_EVENEMENTSController(GenericCrud<TTE_TYPE_EVENEMENTS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TETQ_ETIQUETTESController : GenericCrudController<TETQ_ETIQUETTES, KrialysDbContext>
{
    public TETQ_ETIQUETTESController(GenericCrud<TETQ_ETIQUETTES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TOBF_OBJ_FORMATSController : GenericCrudController<TOBF_OBJ_FORMATS, KrialysDbContext>
{
    public TOBF_OBJ_FORMATSController(GenericCrud<TOBF_OBJ_FORMATS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TOBJE_OBJET_ETIQUETTESController : GenericCrudController<TOBJE_OBJET_ETIQUETTES, KrialysDbContext>
{
    public TOBJE_OBJET_ETIQUETTESController(GenericCrud<TOBJE_OBJET_ETIQUETTES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TOBN_OBJ_NATURESController : GenericCrudController<TOBN_OBJ_NATURES, KrialysDbContext>
{
    public TOBN_OBJ_NATURESController(GenericCrud<TOBN_OBJ_NATURES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TPRCP_PRC_PERIMETRESController : GenericCrudController<TPRCP_PRC_PERIMETRES, KrialysDbContext>
{
    public TPRCP_PRC_PERIMETRESController(GenericCrud<TPRCP_PRC_PERIMETRES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TSEQ_SUIVI_EVENEMENT_ETQSController : GenericCrudController<TSEQ_SUIVI_EVENEMENT_ETQS, KrialysDbContext>
{
    public TSEQ_SUIVI_EVENEMENT_ETQSController(GenericCrud<TSEQ_SUIVI_EVENEMENT_ETQS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TSR_SUIVI_RESSOURCESController : GenericCrudController<TSR_SUIVI_RESSOURCES, KrialysDbContext>
{
    public TSR_SUIVI_RESSOURCESController(GenericCrud<TSR_SUIVI_RESSOURCES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TTR_TYPE_RESSOURCESController : GenericCrudController<TTR_TYPE_RESSOURCES, KrialysDbContext>
{
    public TTR_TYPE_RESSOURCESController(GenericCrud<TTR_TYPE_RESSOURCES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TACT_ACTIONSController : GenericCrudController<TACT_ACTIONS, KrialysDbContext>
{
    public TACT_ACTIONSController(GenericCrud<TACT_ACTIONS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRGL_REGLESController : GenericCrudController<TRGL_REGLES, KrialysDbContext>
{
    public TRGL_REGLESController(GenericCrud<TRGL_REGLES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRGLI_REGLES_LIEESController : GenericCrudController<TRGLI_REGLES_LIEES, KrialysDbContext>
{
    public TRGLI_REGLES_LIEESController(GenericCrud<TRGLI_REGLES_LIEES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRGLRV_REGLES_VALEURSController : GenericCrudController<TRGLRV_REGLES_VALEURS, KrialysDbContext>
{
    public TRGLRV_REGLES_VALEURSController(GenericCrud<TRGLRV_REGLES_VALEURS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TOBJR_OBJET_REGLESController : GenericCrudController<TOBJR_OBJET_REGLES, KrialysDbContext>
{
    public TOBJR_OBJET_REGLESController(GenericCrud<TOBJR_OBJET_REGLES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TETQR_ETQ_REGLESController : GenericCrudController<TETQR_ETQ_REGLES, KrialysDbContext>
{
    public TETQR_ETQ_REGLESController(GenericCrud<TETQR_ETQ_REGLES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TTDOM_TYPE_DOMAINESController : GenericCrudController<TTDOM_TYPE_DOMAINES, KrialysDbContext>
{
    public TTDOM_TYPE_DOMAINESController(GenericCrud<TTDOM_TYPE_DOMAINES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TDOM_DOMAINESController : GenericCrudController<TDOM_DOMAINES, KrialysDbContext>
{
    public TDOM_DOMAINESController(GenericCrud<TDOM_DOMAINES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class VACCGET_ACCUEIL_GRAPHE_ETQSController : GenericCrudController<VACCGET_ACCUEIL_GRAPHE_ETQS, KrialysDbContext>
{
    public VACCGET_ACCUEIL_GRAPHE_ETQSController(GenericCrud<VACCGET_ACCUEIL_GRAPHE_ETQS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class ETQ_TM_AET_AuthorizationController : GenericCrudController<ETQ_TM_AET_Authorization, KrialysDbContext>
{
    public ETQ_TM_AET_AuthorizationController(GenericCrud<ETQ_TM_AET_Authorization, KrialysDbContext> genericService) : base(genericService) { }
}