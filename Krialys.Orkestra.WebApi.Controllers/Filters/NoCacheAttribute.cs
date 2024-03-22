using Microsoft.AspNetCore.Mvc.Filters;

namespace Krialys.Orkestra.WebApi.Controllers.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class NoCacheAttribute : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        var headers = context.HttpContext.Response.Headers;

        headers.Add("Cache-Control", "no-store, no-cache, must-revalidate, max-age=0");
        headers.Add("Pragma", "no-cache");
        headers.Add("Expires", "0");

        base.OnResultExecuting(context);
    }
}