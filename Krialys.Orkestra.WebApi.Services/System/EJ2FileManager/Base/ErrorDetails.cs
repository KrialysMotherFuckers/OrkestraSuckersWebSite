#if EJ2_DNX
using System.Web;
#endif

namespace Krialys.Orkestra.WebApi.Services.EJ2FileManager.Base;

public class ErrorDetails
{
    public string Code { get; set; }

    public string Message { get; set; }

    public IEnumerable<string> FileExists { get; set; }
}