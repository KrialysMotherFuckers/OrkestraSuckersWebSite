using Hangfire;
using Hangfire.Heartbeat;
using Hangfire.MemoryStorage;
using Hangfire.StackTrace;
using HealthChecks.UI.Client;
using Krialys.Common.Literals;
using Krialys.Common.Localization;
using Krialys.Data.EF.RefManager;
using Krialys.Data.EF.Resources;
using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.WebApi.Infrastructure.Common;
using Krialys.Orkestra.WebApi.Infrastructure.Common.Services;
using Krialys.Orkestra.WebApi.Jobs;
using Krialys.Orkestra.WebApi.Services.Auth;
using Krialys.Orkestra.WebApi.Services.Common.Factories;
using Krialys.Orkestra.WebApi.Services.DI;
using Krialys.Orkestra.WebApi.Services.System.HUB;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace Krialys.Orkestra.WebApi.Infrastructure;

public static class Startup
{
    public static IServiceCollection AddOrkestraWebapiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Swagger
        if (configuration.GetValue<bool>("Swagger:Enable"))
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc(Litterals.Version1, new OpenApiInfo()
                {
                    Version = Litterals.Version1,
                    Title = $"Orkestra-{configuration["Environment"]} APIs",
                    Description = $"List of exposed APIs of the Orkestra-{configuration["Environment"]} platform.",
                    //TermsOfService = new Uri("https://orkestra-data.com"),
                    Contact = new OpenApiContact()
                    {
                        Name = "Krialys",
                        //Email = "stephane.juillard@krialys.com",
                        Url = new Uri("https://orkestra-data.com"),
                    },
                    /* => TODO: put these informations when available
                    License = new OpenApiLicense()
                    {
                        Name = "Private",
                        Url = new Uri("https://orkestra-data.com/?contact=xxx"),
                    }*/
                });

                opt.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                opt.IgnoreObsoleteActions();
                opt.IgnoreObsoleteProperties();
                opt.CustomSchemaIds(t => t.FullName);

                opt.AddSecurityDefinition(Litterals.TokenBearer, new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Scheme = "Authorization",
                    BearerFormat = Litterals.TokenBearer,
                    Type = SecuritySchemeType.ApiKey,
                    Description = $"JWT Authorization header using the Bearer format.\r\n\r\nEnter '{Litterals.TokenBearer}' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer xxxx\"",
                });

                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = Litterals.TokenBearer
                                }
                            },
                            new string[] {}
                        }
                    });
            });
        }

        // HangFire
        services.AddHangfire(
                configuration => configuration
                   .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                   .UseSimpleAssemblyNameTypeSerializer()
                   .UseRecommendedSerializerSettings()
                   .UseHeartbeatPage(checkInterval: TimeSpan.FromSeconds(1))
                   .UseStackTrace()
                   .UseMemoryStorage());
        services.AddHangfireServer();
        services.AddHostedService<HangfireWorker>();

        services.AddResponseCaching();

        // Uses CORS (WASM needs that, we can refine to scope to a list of allowed URLs) see https://code-maze.com/blazor-webassembly-httpclient/
        // Refer to this article if you require more information on CORS see https://docs.microsoft.com/en-us/aspnet/core/security/cors
        // https://github.com/MirzaMerdovic/DotNetCore-WebApiStarte
        static void Build(CorsPolicyBuilder b)
        {
            b.WithOrigins("*").WithMethods("*").WithHeaders("*").Build();
        }
        services.AddCors(options =>
        {
            options.AddPolicy("Open", Build);
        });

        // Fid
        if (configuration.GetValue<bool>("FidKit:UseFid"))
        {
            // Fédération d'identité
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = configuration["FidKit:Authority"];
                options.CallbackPath = configuration["FidKit:CallbackPath"];
                options.ClientId = configuration["FidKit:ClientId"];
                options.ClientSecret = configuration["FidKit:ClientSecret"];
                options.ResponseType = configuration["FidKit:ResponseType"];
                options.SaveTokens = true;

                // Scope
                string _scope = configuration["FidKit:Scope"];
                if (!string.IsNullOrWhiteSpace(_scope))
                {
                    options.Scope.Clear();
                    _scope.Split(" ", StringSplitOptions.TrimEntries).ToList().ForEach(item =>
                    {
                        options.Scope.Add(item);
                    });
                }

                // Claims
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;

                // ClaimActions
                string _claimActions = configuration["FidKit:ClaimActions"];
                if (!string.IsNullOrWhiteSpace(_claimActions))
                {
                    _claimActions.Split(" ", StringSplitOptions.TrimEntries).ToList().ForEach(item =>
                    {
                        options.ClaimActions.MapUniqueJsonKey(item, item);
                    });
                }
            });
        }

        // Authentication service using a JWT.
        services.AddAuthentication("OAuthScheme")
            .AddJwtBearer("OAuthScheme", config =>
            {
                /* Get secret signing key. */
                var tokenSigningKey = configuration["Authentication:TokenSigningKey"] ?? "TokenSigningKey";
                /* Token key used for signing. */
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSigningKey));

                /* Describe expected parameters to validate a token. */
                config.TokenValidationParameters = new TokenValidationParameters
                {
                    /* Validated parameters. */
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    /* The default value of ClockSkew is 5 minutes. That means if you haven't set it, your token will be still valid for up to 5 minutes. */
                    ClockSkew = TimeSpan.Zero,
                    /* Accepted issuer. */
                    ValidIssuer = configuration[configuration["Authentication:Issuer"] ?? string.Empty],
                    /* Accepted audience. */
                    ValidAudience = configuration[configuration["Authentication:Audience"] ?? string.Empty],
                    /* Key used for signing. */
                    IssuerSigningKey = key
                };

                /* Events invoked by JWT service. */
                config.Events = new JwtBearerEvents
                {
                    /* If authentication fails, add a "Token-Expired" field in response header.
                     * Used to differentiate cases where user is not authorized from case where
                     * access token is incorrect and may need to be refreshed. */
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        /* Add credential service.
         * This service is used to generate access codes and tokens. */
        services.TryAddScoped<IAuthentificationServices, AuthentificationServices>();

        // Register codepages
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = uint.MaxValue; // 4 GB limit
            options.ValueLengthLimit = int.MaxValue; // 2 GB limit
        });

        // If using Kestrel:
        services.Configure<KestrelServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
            // 1 GB limit
            // options.Limits.MaxRequestBodySize = 64 * 1024; // ** optimization ** default is a 30 Mb buffer, here set only to 64 Kb
            options.Limits.MaxRequestBodySize = 1048576000;

        });

        // If using IIS:
        services.Configure<IISServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
            // 1 GB limit
            // options.Limits.MaxRequestBodySize = 64 * 1024; // ** optimization ** default is a 30 Mb buffer, here set only to 64 Kb
            options.MaxRequestBodySize = 1048576000;
        });

        // Globalization as Singleton => appsettings.json "CultureInfo" => ApiUnivers\Librairies\05-LibRazor\Resources\*.yml
        services.AddLanguageContainer(Assembly.GetAssembly(typeof(ILanguageContainerService)), CultureInfo.GetCultureInfo(configuration["CultureInfo"] ?? string.Empty));

        #region PUWorker aka Client ParalellU worker as service

        // SignalR support as Singleton https://docs.microsoft.com/fr-fr/aspnet/core/signalr/configuration?view=aspnetcore-3.1&tabs=dotnet
        services.AddSignalR(hubOptions =>
        {
            hubOptions.EnableDetailedErrors = true;
            hubOptions.MaximumReceiveMessageSize = 1024 * 512;
        });

        #endregion PUWorker aka Client ParalellU worker as service

        // Add OData Query Settings and valiadtion settings as Singleton
        static ODataValidationSettings ValidationSettingFactory(IServiceProvider sp = null)
            => new()
            {
                MaxExpansionDepth = 0,
                AllowedFunctions = AllowedFunctions.AllFunctions,
                AllowedLogicalOperators = AllowedLogicalOperators.All,
                AllowedArithmeticOperators = AllowedArithmeticOperators.All,
                AllowedQueryOptions = AllowedQueryOptions.All,
                MaxSkip = Globals.MaxTop,
                MaxTop = Globals.MaxTop
            };
        services.TryAddSingleton(ValidationSettingFactory);

        // Localization
        services.AddLocalization();
        services.Configure<RequestLocalizationOptions>(options =>
        {
            // Set the default culture
            options.DefaultRequestCulture = new RequestCulture(configuration["CultureInfo"] ?? string.Empty);
            // Define the list of cultures your app will support
            IList<CultureInfo> supportedCultures = new List<CultureInfo>
            {
                new CultureInfo(configuration["CultureInfo"] ?? string.Empty),
                new CultureInfo(CultureLiterals.FrenchFR),
                new CultureInfo(CultureLiterals.EnglishUS)
            };

            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        // Better MVC support \\
        services.AddMvc(options =>
        {
            options.InputFormatters.Insert(0, new RawJsonBodyInputFormatter()); // for POST && PUT && PATCH, mandatory for //U !

            options.EnableEndpointRouting = false;

            // Refer to this article for more details on how to properly set the caching for your needs
            // https://docs.microsoft.com/en-us/aspnet/core/performance/caching/response
            options.CacheProfiles.Add(
                "default",
                new CacheProfile
                {
                    Duration = 60,
                    Location = ResponseCacheLocation.Client
                });
        })
        .AddDataAnnotationsLocalization(options =>
        {
            options.DataAnnotationLocalizerProvider = (_, factory) =>
                factory.Create(typeof(DataAnnotationsResources));
        })
        .AddJsonOptions(options => // These options have been ported into Krialys.Orkestra.Common
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = Krialys.Orkestra.Common.JsonOptions.Options.PropertyNamingPolicy;
            options.JsonSerializerOptions.NumberHandling = Krialys.Orkestra.Common.JsonOptions.Options.NumberHandling;
            options.JsonSerializerOptions.ReferenceHandler = Krialys.Orkestra.Common.JsonOptions.Options.ReferenceHandler;
#if RELEASE
            options.JsonSerializerOptions.DefaultIgnoreCondition = Krialys.Orkestra.Common.JsonOptions.Options.DefaultIgnoreCondition;
#endif
            foreach (var converter in Krialys.Orkestra.Common.JsonOptions.Options.Converters)
            {
                options.JsonSerializerOptions.Converters.Add(converter);
            }
        });

        // Configures the MVC services for the commonly used features with controllers with views + razor page support runtime compilation
        var mvcBuilder = services.AddControllers(options =>
        {
            options.InputFormatters.Insert(0, JsonPatchInputFormatter.GetJsonPatchInputFormatter());
        });

        // Register all Entities as Services (DI)
        InjectDbContext(services, mvcBuilder, configuration);

        services.AddBusinessServices();
        services.AddHttpContextAccessor();

        // Use compression
        services.AddResponseCompression(options =>
        {
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream", "image/svg+xml", "application/atom+xml" });
        });
        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        services.AddServices();

        return services;
    }

    public static IApplicationBuilder UseOrkestraWebapiServices(this IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("Swagger:Enable"))
        {
            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint(env.IsDevelopment() ? "/swagger/v1/swagger.json" : "v1/swagger.json", $"Krialys.Orkestra.WebApi"));
        }

        if (configuration.GetValue<bool>("Hangfire:ShowDashboard"))
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                DashboardTitle = "Orkestra Jobs",
                Authorization = new[] { new HangfireAuthorizationFilter("admin") }
            });
        }

        // Use compression
        app.UseResponseCompression();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseODataRouteDebug();
        }

        app.UseRequestLocalization();
        app.UseHealthChecks("/hc", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        #region CONFIGURE ROUTING

        app.UseODataBatching();
        app.UseRouting();
        app.UseCors("Open");
        app.UseResponseCaching();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();

            //endpoints.MapBlazorHub(); => crash serializer as of Net Core 6!

            // ** ParallelU MASTER EndPoint ** \\
            if (!string.IsNullOrEmpty(CPULitterals.CpuHub))
            {
                endpoints.MapHub<SPUHub>(CPULitterals.CpuHub, options =>
                {
                    options.ApplicationMaxBufferSize = 1024 * 256;
                    options.TransportMaxBufferSize = 1024 * 256;
                });
                endpoints.MapHub<ChatHub>(CPULitterals.ChatHub, options =>
                {
                    options.ApplicationMaxBufferSize = 1024 * 256;
                    options.TransportMaxBufferSize = 1024 * 256;
                });
            }

            // ** HealthChecks EndPoint ** \\
            if (configuration.GetValue<bool>("HealthChecks:Enable"))
            {
                endpoints.MapHealthChecks("/hc");
            }

            endpoints.MapFallbackToFile("index.html");
        });

        #endregion CONFIGURE ROUTING

        return app;
    }

    #region DBCONTEXT INJECTION

    private static void InjectDbContext(IServiceCollection services, IMvcBuilder mvcCoreBuilder, IConfiguration configuration)
    {
        // Chemin racine de l'exécutable
        var contentRoot = Globals.AssemblyDirectory;

        // Add DBContexts + inject specific services
        foreach (var e in configuration.GetValue<string>("ConnectionStrings:DBList").Split('|'))
        {
            var dbSlot = e.Split('@')[0]; // => ex: DBMSO
            var dbType = e.Split('@')[1]; // => ex: SQLite
            var csName = configuration.GetValue<string>($"ConnectionStrings:{dbSlot}:{dbType}:Name").Replace("{contentRoot}", contentRoot, StringComparison.OrdinalIgnoreCase);

            // Force to connect to the correct database not the ones under csproj but in the binaries folder
            if (dbType.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
            {
                var builder = new SqliteConnectionStringBuilder(csName);
                builder.DataSource = Path.Combine(Globals.AssemblyDirectory, builder.DataSource);
                csName = builder.ToString();
            }

            // Registers required services health checks for all DB \\
            if (configuration.GetValue<bool>("HealthChecks:Enable"))
            {
                _ = services.AddHealthChecks()
                    // Health check for all connected databases
                    .AddCheck(
                        dbSlot,
                        new SqlConnectionHealthCheck(csName, dbType),
                        HealthStatus.Unhealthy,
                        new[] { dbType, $"Service started at: {DateTime.Now:g}" }
                    );
            }

            // https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-2.0/#dbcontext-pooling
            if (dbSlot.Equals("DBREFMANAGER", StringComparison.OrdinalIgnoreCase))
            {
                _ = services.AddDbContextPool<KrialysDbContext>(options =>
                {
                    if (dbType.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
                    {
                        options.UseSqlite(csName, sqlOptions => { sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery); });
                        options.EnableServiceProviderCaching();
                    }
                    //else
                    //{
                    //    _ = dbType.Equals("SQLSERVER", StringComparison.OrdinalIgnoreCase)
                    //        ? options.UseSqlServer(csName, sqlOptions =>
                    //                                {
                    //                                    sqlOptions.EnableRetryOnFailure(
                    //                                    maxRetryCount: 10,
                    //                                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    //                                    errorNumbersToAdd: null);
                    //                                })
                    //        : throw new ArgumentNullException($"STORE-DBMS: {dbType} not found!");
                    //}
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
                    options.ConfigureWarnings(x =>
                        x.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning, RelationalEventId.MultipleCollectionIncludeWarning));
                    options.EnableDetailedErrors();
                });

                // DBSTORE DI \\
                ODataInjection.AddRefManagerEdmModel(services, mvcCoreBuilder, dbSlot, dbType);
            }
            else if (dbSlot.Equals("DBMSO", StringComparison.OrdinalIgnoreCase))
            {
                _ = services.AddDbContextPool<Krialys.Data.EF.Mso.KrialysDbContext>(options =>
                {
                    if (dbType.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
                    {
                        options.UseSqlite(csName, sqlOptions => { sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery); });
                        options.EnableServiceProviderCaching();
                    }
                    //else
                    //{
                    //    _ = dbType.Equals("SQLSERVER", StringComparison.OrdinalIgnoreCase)
                    //        ? options.UseSqlServer(csName, sqlOptions =>
                    //                                {
                    //                                    sqlOptions.EnableRetryOnFailure(
                    //                                    maxRetryCount: 10,
                    //                                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    //                                    errorNumbersToAdd: null);
                    //                                })
                    //        : throw new ArgumentNullException($"MSO-DBMS: {dbType} not found!");
                    //}
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
                    options.ConfigureWarnings(x =>
                        x.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning, RelationalEventId.MultipleCollectionIncludeWarning));
                    options.EnableDetailedErrors();
                });

                // DBMSO DI \\
                ODataInjection.AddMsoEdmModel(services, mvcCoreBuilder, dbSlot, dbType);
            }
            else if (dbSlot.Equals("DBUNIVERS", StringComparison.OrdinalIgnoreCase))
            {
                _ = services.AddDbContextPool<Krialys.Data.EF.Univers.KrialysDbContext>(options =>
                {
                    if (dbType.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
                    {
                        options.UseSqlite(csName, sqlOptions => { sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery); });
                        options.EnableServiceProviderCaching();
                    }
                    //else
                    //{
                    //    _ = dbType.Equals("SQLSERVER", StringComparison.OrdinalIgnoreCase)
                    //        ? options.UseSqlServer(csName, sqlOptions =>
                    //                                {
                    //                                    sqlOptions.EnableRetryOnFailure(
                    //                                    maxRetryCount: 10,
                    //                                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    //                                    errorNumbersToAdd: null);
                    //                                })
                    //        : throw new ArgumentNullException($"UNIVERS-DBMS: {dbType} not found!");
                    //}
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
                    options.ConfigureWarnings(x =>
                        x.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning, RelationalEventId.MultipleCollectionIncludeWarning));
                    options.EnableDetailedErrors();
                });

                // DBUNIVERS DI \\
                ODataInjection.AddUniversEdmModel(services, mvcCoreBuilder, dbSlot, dbType);
            }
            else if (dbSlot.Equals("DBLOGS", StringComparison.OrdinalIgnoreCase))
            {
                _ = services.AddDbContextPool<Krialys.Data.EF.Logs.KrialysDbContext>(options =>
                {
                    if (dbType.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
                    {
                        options.UseSqlite(csName, sqlOptions => { sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery); });
                        options.EnableServiceProviderCaching();
                    }
                    //else
                    //{
                    //    _ = dbType.Equals("SQLSERVER", StringComparison.OrdinalIgnoreCase)
                    //        ? options.UseSqlServer(csName, sqlOptions =>
                    //                                {
                    //                                    sqlOptions.EnableRetryOnFailure(
                    //                                    maxRetryCount: 10,
                    //                                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    //                                    errorNumbersToAdd: null);
                    //                                })
                    //        : throw new ArgumentNullException($"LOGS-DBMS: {dbType} not found!");
                    //}
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
                    options.ConfigureWarnings(x =>
                        x.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning, RelationalEventId.MultipleCollectionIncludeWarning));
                    options.EnableDetailedErrors();
                });

                // DBLOGS DI \\
                ODataInjection.AddLogsEdmModel(services, mvcCoreBuilder, dbSlot, dbType);
            }

            else if (dbSlot.Equals("DBETQ", StringComparison.OrdinalIgnoreCase))
            {
                _ = services.AddDbContextPool<Krialys.Data.EF.Etq.KrialysDbContext>(options =>
                {
                    if (dbType.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
                    {
                        options.UseSqlite(csName, sqlOptions => { sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery); });
                        options.EnableServiceProviderCaching();
                    }
                    //else
                    //{
                    //    _ = dbType.Equals("SQLSERVER", StringComparison.OrdinalIgnoreCase)
                    //        ? options.UseSqlServer(csName, sqlOptions =>
                    //                                {
                    //                                    sqlOptions.EnableRetryOnFailure(
                    //                                    maxRetryCount: 10,
                    //                                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    //                                    errorNumbersToAdd: null);
                    //                                })
                    //        : throw new ArgumentNullException($"ETQ-DBMS: {dbType} not found!");
                    //}
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
                    options.ConfigureWarnings(x =>
                        x.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning, RelationalEventId.MultipleCollectionIncludeWarning));
                    options.EnableDetailedErrors();
                });

                // DBETQ DI \\
                ODataInjection.AddEtqEdmModel(services, mvcCoreBuilder, dbSlot, dbType);
            }
            else if (dbSlot.Equals("DbFileStorage", StringComparison.OrdinalIgnoreCase))
            {
                _ = services.AddDbContextPool<Data.EF.FileStorage.KrialysDbContext>(options =>
                {
                    if (dbType.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
                    {
                        options.UseSqlite(csName, sqlOptions => { sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery); });
                        options.EnableServiceProviderCaching();
                    }
                    //else
                    //{
                    //    _ = dbType.Equals("SQLSERVER", StringComparison.OrdinalIgnoreCase)
                    //        ? options.UseSqlServer(csName, sqlOptions =>
                    //                                {
                    //                                    sqlOptions.EnableRetryOnFailure(
                    //                                    maxRetryCount: 10,
                    //                                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    //                                    errorNumbersToAdd: null);
                    //                                })
                    //        : throw new ArgumentNullException($"ETQ-DBMS: {dbType} not found!");
                    //}
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
                    options.ConfigureWarnings(x =>
                        x.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning, RelationalEventId.MultipleCollectionIncludeWarning));
                    options.EnableDetailedErrors();
                });

                // FileStorage DI \\
                ODataInjection.AddFileStorageEdmModel(services, mvcCoreBuilder, dbSlot, dbType);
            }
            else
            {
                throw new ArgumentNullException($"DbSlot : No parameter found for {dbSlot}");
            }
        }

        // Global functions DI \\
        ODataInjection.Add(services, mvcCoreBuilder, "SVCOMMON", "__COMMON_SERVICES__");

        // SqlKata Sqlite query factory DI \\
        services.AddScoped<ISqliteQueryFactory, SqliteQueryFactory>();
    }

    #endregion DBCONTEXT INJECTION

    public static bool EfMigrationUpdate(IHost host, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        // Apply EF Migrations
        using var scope = serviceProvider.CreateScope();
        using var dbEtq = scope.ServiceProvider.GetService<Krialys.Data.EF.Etq.KrialysDbContext>();
        using var dbRefManager = scope.ServiceProvider.GetService<Krialys.Data.EF.RefManager.KrialysDbContext>();
        using var dbLogs = scope.ServiceProvider.GetService<Krialys.Data.EF.Logs.KrialysDbContext>();
        using var dbMso = scope.ServiceProvider.GetService<Krialys.Data.EF.Mso.KrialysDbContext>();
        using var dbUnivers = scope.ServiceProvider.GetService<Krialys.Data.EF.Univers.KrialysDbContext>();
        using var dbFileStorage = scope.ServiceProvider.GetService<Data.EF.FileStorage.KrialysDbContext>();

        try
        {
            dbLogs?.Database.Migrate();
            dbEtq?.Database.Migrate();
            dbRefManager?.Database.Migrate();
            dbMso?.Database.Migrate();
            dbUnivers?.Database.Migrate();
            dbFileStorage?.Database.Migrate();

            dbLogs?.SetPragmas();
            dbEtq?.SetPragmas();
            dbRefManager?.SetPragmas();
            dbMso?.SetPragmas();
            dbUnivers?.SetPragmas();
            dbFileStorage?.SetPragmas();

            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"ApiUnivers encountered a migration exception: {ex.Message}", EventLogEntryType.Error);
            host.StopAsync();
        }

        return false;
    }

    public static void SmtpCheck(IConfiguration configuration)
    {
        if (string.IsNullOrEmpty(configuration["MailKit:SMTP:Host"]))
            Log.Logger.Warning($"> SMTP host is not configured for APIUNIVERS-{configuration["Environment"]}@{Environment.MachineName}");
        else
            Log.Logger.Information($"> SMTP host is configured for APIUNIVERS-{configuration["Environment"]}@{Environment.MachineName}");
    }

    /// <summary>
    /// https://phiresky.github.io/blog/2020/sqlite-performance-tuning/
    /// </summary>
    private static void SetPragmas(this DbContext dbContext)
    {
        using var ctx = dbContext.Database.GetDbConnection();
        ctx.Open();
        using (var command = ctx.CreateCommand())
        {
            command.CommandText = "PRAGMA journal_mode=MEMORY; PRAGMA synchronous=normal; PRAGMA temp_store=memory; PRAGMA mmap_size=30000000000; PRAGMA auto_vacuum=INCREMENTAL; PRAGMA incremental_vacuum;";
            command.ExecuteNonQuery();

            // Check the current vaccuum state
            command.CommandText = "PRAGMA auto_vacuum;";

            // Apply the incremental vacuum flag by vaccuuming the db first (and only once)
            if (Convert.ToInt32(command.ExecuteScalar() ?? 0) != 2)
            {
                command.CommandText = "VACUUM;";
                command.ExecuteNonQuery();
            }

            command.CommandText = "PRAGMA foreign_keys=ON; PRAGMA foreign_key_check; PRAGMA optimize;";
            command.ExecuteNonQuery();
        }
        ctx.Close();
    }
}