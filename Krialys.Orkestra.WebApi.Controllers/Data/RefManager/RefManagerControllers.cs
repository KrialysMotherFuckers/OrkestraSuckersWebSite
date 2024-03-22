using Krialys.Data.EF.RefManager;
using Krialys.Orkestra.WebApi.Controllers.Common;
using Krialys.Orkestra.WebApi.Services.Common;

namespace Krialys.Orkestra.WebApi.Controllers.RefManager;

/// <summary>
/// This class represents the route to be applied
/// </summary>
internal static class Route { internal const string Name = Litterals.RefManagerRootPath; }

[ApiExplorerSettings]
[Area(Route.Name)]
public class TM_RFS_ReferentialSettingsController : GenericCrudController<TM_RFS_ReferentialSettings, KrialysDbContext>
{
    public TM_RFS_ReferentialSettingsController(GenericCrud<TM_RFS_ReferentialSettings, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings]
[Area(Route.Name)]
public class TR_CNX_ConnectionsController : GenericCrudController<TR_CNX_Connections, KrialysDbContext>
{
    public TR_CNX_ConnectionsController(GenericCrud<TR_CNX_Connections, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings]
[Area(Route.Name)]
public class TM_RFH_ReferentialHistoricalController : GenericCrudController<TM_RFH_ReferentialHistorical, KrialysDbContext>
{
    public TM_RFH_ReferentialHistoricalController(GenericCrud<TM_RFH_ReferentialHistorical, KrialysDbContext> genericService) : base(genericService) { }
}