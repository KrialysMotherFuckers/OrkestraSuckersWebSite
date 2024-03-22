using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OData.Edm;
using System.Reflection;

namespace Krialys.Orkestra.WebApi.Services.Extensions;

public static class ODataExtensions
{
    public static void AddTo(this IServiceCollection services, IMvcBuilder builder, string dbSlot, string dbType, IEdmModel model)
    {
        // Official sources: https://github.com/OData/AspNetCoreOData/blob/master/sample/ODataRoutingSample/Startup.cs
        builder
            .ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Remove(manager.FeatureProviders.OfType<ControllerFeatureProvider>().FirstOrDefault());
                manager.FeatureProviders.Add(new RemoveMetadataControllerFeatureProvider());
            })
            .AddOData(opt =>
            {
                opt.Count()
                        .Filter()
                        .Expand()
                        .Select()
                        .OrderBy()
                        .SkipToken()
                        .AddRouteComponents($"api/{dbSlot[2..]}/v1", model, s =>
                            s.AddSingleton<ODataBatchHandler, DefaultODataBatchHandler>()
                            ).TimeZone = TimeZoneInfo.Utc;
            });
    }

    public class RemoveMetadataControllerFeatureProvider : ControllerFeatureProvider
    {
        protected override bool IsController(TypeInfo typeInfo)
        {
            if (typeInfo.FullName.Equals("Microsoft.AspNetCore.OData.Routing.Controllers.MetadataController", StringComparison.Ordinal))
                return false;

            return base.IsController(typeInfo);
        }
    }
}