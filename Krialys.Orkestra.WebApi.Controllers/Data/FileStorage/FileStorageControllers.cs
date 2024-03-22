using Krialys.Data.EF.FileStorage;
using Krialys.Orkestra.WebApi.Controllers.Common;
using Krialys.Orkestra.WebApi.Services.Common;

// > Implements GenericCRUDController for FileStorage
namespace Krialys.Orkestra.WebApi.Controllers.FileStorage;

/// <summary>
/// This class represents the route to be applied
/// </summary>
internal static class Route { internal const string Name = Litterals.FileStorageRootPath; }

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TM_STF_StorageFileRequestController : GenericCrudController<TM_STF_StorageFileRequest, KrialysDbContext>
{
    public TM_STF_StorageFileRequestController(GenericCrud<TM_STF_StorageFileRequest, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TR_SCT_StreamCategoryTypeController : GenericCrudController<TR_SCT_StreamCategoryType, KrialysDbContext>
{
    public TR_SCT_StreamCategoryTypeController(GenericCrud<TR_SCT_StreamCategoryType, KrialysDbContext> genericService) : base(genericService) { }
}