#if EJ2_DNX
using System.Web;
#endif

namespace Krialys.Orkestra.WebApi.Services.EJ2FileManager.Base;

public abstract class ImageSize
{
    public int Height { get; set; }
    public int Width { get; set; }
}