#if EJ2_DNX
using System.Web;
#endif

namespace Krialys.Orkestra.WebApi.Services.EJ2FileManager.Base;

public class FileManagerResponse
{
    public FileManagerDirectoryContent Cwd { get; set; }
    public IEnumerable<FileManagerDirectoryContent> Files { get; set; }
    public ErrorDetails Error { get; set; }
    public FileDetails Details { get; set; }
}