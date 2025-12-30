using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EZ.Connector.Init.Helpers;
using EZGO.Api.Data;
using EZGO.Api.Data.Helpers;
using EZGO.Api.Interfaces.Data;
//using EZGO.Api.Legacy.Helpers;
using EZGO.Api.Logic.Helpers;
using EZGO.Api.Logic.Exporting.Helpers;
using EZGO.Api.Logic.Provisioner.Helpers;
using EZGO.Api.Logic.SapPmConnector.Helpers;
using EZGO.Api.Security.Helpers;
using EZGO.Api.Migrations.Helpers;
using EZGO.Api.Utils.Helpers;
using EZGO.Api.Utils.Logging;
using EZGO.Api.Utils.Middleware;
using EZGO.Api.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using EZGO.Api.Settings.Helpers;
using Microsoft.Extensions.FileProviders;
using System.IO;
using EZGO.Api.TaskGeneration.Helpers;
using Elastic.Apm.AspNetCore;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.NetCoreAll;
using EZ.Connector.Ultimo.Helpers;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static Elastic.Apm.Config.ConfigConsts;
using System.Collections;
using Microsoft.OpenApi.Models;
using EZGO.Api.Middleware;
using System.Reflection;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.Extensions.Options;
using EZGO.Api.Utils.AspNetCore.Helpers;
using EZGO.Api.Logic.Templates.Helpers;
using System;
using EZGO.Api.Helper;

namespace EZGO.Api
{
    public class Startup
    {
        ILogger _logger;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // TODO check if it ok to add the authentication to the security extension. Maybe for aesthetic purposes its better to leave it in here.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.ModelBinderProviders.RemoveType<DateTimeModelBinderProvider>(); //remove datetime binder due to incompatibility
                options.Filters.Add<FeatureGlobalFilter>(); //add feature filter
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            }); //.NetCore controller usage.

            //TODO refactor a bit
            if (Configuration.GetSection("AppSettings:EnvironmentConfig").Value == "development" ||
                    Configuration.GetSection("AppSettings:EnvironmentConfig").Value == "localdevelopment" ||
                    Configuration.GetSection("AppSettings:EnvironmentConfig").Value == "test")
            {


                List<string> xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();

                //only add swagger services for development, localdev and test
                services.AddSwaggerGen(options =>
                {
                    //add basic swagger docs
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "EZGO API",
                        Version = "v1",
                        Description = "EZGO API for CMS, Web and Xamarin clients",
                        Contact = new OpenApiContact
                        {
                            Name = "EZ Factory",
                            Email = string.Empty,
                            Url = new Uri("https://ezfactory.nl/")
                        }, 
                    });

                    //change schema generation to make enums into an object with multiple properties for the front-end types
                    options.SchemaFilter<EnumSchemaFilter>();
                    
                    //add security requirements and defs for usage of bearer tokens
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Security token",
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        BearerFormat = "JWT",
                        Scheme = "bearer"
                    });
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type=ReferenceType.SecurityScheme,
                                    Id="Bearer"
                                }
                            },
                            new string[]{}
                        }
                    });

                    foreach (string fileName in xmlFiles)
                    {
                        if (File.Exists(fileName))
                            options.IncludeXmlComments(fileName, includeControllerXmlComments: true);
                    }

                    //Add auth tag to route docs
                    options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                    //Add required headers for use with API
                    options.OperationFilter<AddRequiredHeaderParameter>(); // Add this here
                });
            }
            
            services.AddHttpContextAccessor(); //.NetCore Context Accessors.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(
                                    Encoding.ASCII.GetBytes(Configuration.GetSection(AuthenticationSettings.SECURITY_TOKEN_CONFIG_KEY).Value)),
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero
                        };
                    }); //Add Authentication for use with tokens.

            services.AddMemoryCache();
            services.AddSingleton<RateLimiterHelper>();            
            services.AddSettingServices(); //Settings services, contains DI manager that are needed for the application. These DI managers or helpers are based around setttings and configuraitons.
            services.AddDataServices(Configuration); //Data Services, contains DI managers that are needed for the application. If a manager that is used for DI is added within the data library, add it to the services in this method.
            services.AddLogicDataServices();
            services.AddSecurityServices(); //Security Services, contains DI managers that are needed for the application. If a manager that is used for DI is added within the security library, add it to the services in this method.
            services.AddUtilServices(); //UtilServices, contains DI managers that are needed for the application. If a manager that is used for DI is added within the Util library, add it to the services in this method.
            services.AddUtilAspNetCoreServices();
            services.AddLogicServices(); //Logic Services, contains DI managers that are needed for the application. If a manager that is used for DI is added within the logic library, add it to the services in this method.
            services.AddLogicTemplatesServices();
            services.AddTaskGenerationServices();
            services.AddExportingServices();
            services.AddProvisionerServices(); //Add provisioner services, primarily used in the provisioner worker service but some functionalitys can be called within the api. 
            services.AddConnectorServices();//Add Connector services for SAP, Ultimo etc.
            services.AddDatabaseMigrationServices(); //Add migration services to run database migations.
            services.AddSapPMConnectionServices(); //Add SAP PM connection services for SAP PM connections.
      

            services.AddCors(options =>
            {
                options.AddPolicy("CorrsPolicy",
                    builder =>
                    {
                        builder.WithOrigins(this.GetCorrs())
                                            .AllowAnyHeader()
                                            .AllowAnyMethod();
                    });
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Application Builder used by core3.1</param>
        /// <param name="env">Hosting environment used for release/dev settings</param>
        /// <param name="loggerFactory">Logger factory used for logging within providers and certain structures</param>
        /// <param name="dblogWriter">Log writer for writing simple logs to DB used within log provider and middlewares.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IDatabaseLogWriter dblogWriter)
        {
            app.UseAllElasticApm(Configuration); //enable Elestic.co APM stack.
           
            //Use for specific:  app.UseElasticApm(Configuration, new HttpDiagnosticsSubscriber()); //enable Elestic.co APM stack.

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //Note! Database logger is based on the ILogger and ILoggerProvider constructs.
            //See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1
            loggerFactory.AddProvider(new DatabaseLoggerProvider(Configuration, dblogWriter)); //Add custom database logger.

            //Only enable on older stacks that contain direct files. (new-new stack will run on k8s with S3 only.)
            if(Configuration.GetSection("AppSettings:EnableDiskUpload") != null && Convert.ToBoolean(Configuration.GetSection("AppSettings:EnableDiskUpload").Value))
            {
                try
                {
                    //Static files enabled based on the media directory, static files are not handled by the normal .net core request pipe.
                    app.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(
                                           Path.Combine(Directory.GetCurrentDirectory(), "media")),
                        RequestPath = "/media"
                    });
                } catch (Exception ex)
                {
                    _logger = loggerFactory.CreateLogger<Startup>();
                    _logger.LogError("Static Path can not be added [{0}]", ex.Message);
                    //static files can not be initiated. 
                }

            }


            //Add Middleware for specific request handling. Using a general dblogWriter for writing data to the log.
            app.UseMiddleware<RequestResponseMiddleware>(dblogWriter);
            //Add Middleware for generic exception handling.
            app.UseMiddleware<ExceptionMiddleware>();

            app.Use(async (context, next) =>
            {
                try
                {
                    context.Response.Headers.Add("Ez-API-Container", System.Environment.MachineName);
                }
                catch
                {

                }

                await next.Invoke();
            });

            app.UseHttpsRedirection(); //enable redirecton

            app.UseRouting();

            app.UseCors(); //NOTE! must be before UseAuthorization due to header stripping when using authorization.

            app.UseAuthentication(); //NOTE! must be above UseAuthorization!

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //TODO refactor a bit
            if (env.IsDevelopment() || env.IsEnvironment("LocalDevelopment") || env.IsEnvironment("Test"))
            {
                // Only Add swagger on local dev, dev and test.
                // Add swagger auth (basic)
                app.UseSwaggerAuthorized();

                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger(option =>
                {
                    option.RouteTemplate = "ezfdocs/{documentName}/swagger.json";
                });
                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
                app.UseSwaggerUI(option =>
                {
                    option.SwaggerEndpoint("/ezfdocs/v1/swagger.json", "EZGO API");
                    option.RoutePrefix = "ezfdocs";
                    option.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                });
            }

            try
            {
                dblogWriter.WriteToLog(message: string.Format("EZGO Api {0} started for {1} at {2}", GetType().Assembly.GetName().Version.ToString(), env.EnvironmentName, DateTimeOffset.Now),
                       type: "INFORMATION",
                       eventid: "0",
                       eventname: "STARTUP",
                       description: string.Format("API startup on {0}", Environment.MachineName),
                       source: Configuration.GetSection(ApiSettings.APPLICATION_NAME_CONFIG_KEY).Value);

              
                dblogWriter.WriteToLog(message: "Environmental stats",
                            type: "INFORMATION",
                            eventid: "0",
                            eventname: "STARTUP",
                            description: GetEnvironmentalStatistics(),
                            source: Configuration.GetSection(ApiSettings.APPLICATION_NAME_CONFIG_KEY).Value);

#pragma warning disable CS0168 // Variable is declared but never used
            } catch (Exception exception)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                //no logging
            }


        }

        /// <summary>
        /// GetCorrs; Get corrs array
        /// </summary>
        /// <returns></returns>
        private string[] GetCorrs()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(Settings.ApiSettings.CORRS_CONFIG_KEY)))
            {
                return Environment.GetEnvironmentVariable(Settings.ApiSettings.CORRS_CONFIG_KEY).Split(";");
            }
            else return Configuration.GetSection(Settings.ApiSettings.CORRS_CONFIG_KEY).Value.Split(";");

        }

        /// <summary>
        /// AllowLocalhost; Set corrs for allowing localhost.
        /// NOTE! not used, but will probably needed in future. To activiate add custom corrs handler logic and enable though: SetIsOriginAllowed(origin -> AllowLocalhost(origin));
        /// </summary>
        /// <param name="origin">original origin</param>
        /// <returns></returns>
        private bool AllowLocalhost(string origin)
        {
            var uri = new Uri(origin);
            return (uri.Host == "localhost");
        }

        private string GetEnvironmentalStatistics()
        {
            var sb = new StringBuilder();

            foreach (DictionaryEntry e in System.Environment.GetEnvironmentVariables())
            {
                sb.AppendFormat("{0} : {1} ", e.Key, e.Value != null ? e.Value.ToString().Length : "0");
                sb.AppendLine("");
            }
            
            sb.AppendLine(Configuration.GetSection(AuthenticationSettings.SECURITY_TOKEN_CONFIG_KEY).Value.Substring(0, 4));
            sb.AppendLine(Configuration.GetSection(AuthenticationSettings.PROTECTION_CONFIG_KEY).Value.Substring(0, 4));

            return sb.ToString();
        }

    }
}


