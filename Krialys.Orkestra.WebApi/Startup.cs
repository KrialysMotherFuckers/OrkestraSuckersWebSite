using Krialys.Orkestra.WebApi.Infrastructure;

namespace Krialys.Orkestra.WebApi;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration) => _configuration = configuration;

    // This method gets called by the runtime. Use it to add services to the container.
    public void ConfigureServices(IServiceCollection services) => services.AddOrkestraWebapiServices(_configuration);
    
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) => app.UseOrkestraWebapiServices(env, _configuration);
}