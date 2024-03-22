namespace Krialys.Orkestra.WebApi.Services.EJ2FileManager.Base;

public interface IPhysicalFileProviderBase : IScopedService, IFileProviderBase
{
    void RootFolder(string folderName);
}