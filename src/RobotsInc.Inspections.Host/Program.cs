using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using Hellang.Authentication.JwtBearer.Google;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

using RobotsInc.Inspections.API.I.Json;
using RobotsInc.Inspections.API.I.Security;
using RobotsInc.Inspections.BusinessLogic;
using RobotsInc.Inspections.BusinessLogic.Health;
using RobotsInc.Inspections.BusinessLogic.Security;
using RobotsInc.Inspections.BusinessLogic.Util;
using RobotsInc.Inspections.Repositories;
using RobotsInc.Inspections.Repositories.Security;
using RobotsInc.Inspections.Server.API.I;
using RobotsInc.Inspections.Server.Mappers;
using RobotsInc.Inspections.Server.Mappers.Security;
using RobotsInc.Inspections.Server.Security;

using Swashbuckle.AspNetCore.SwaggerGen;

using ArticulatedRobot = RobotsInc.Inspections.Models.ArticulatedRobot;
using AutomatedGuidedVehicle = RobotsInc.Inspections.Models.AutomatedGuidedVehicle;
using Claim = RobotsInc.Inspections.Models.Security.Claim;
using Customer = RobotsInc.Inspections.Models.Customer;
using Inspection = RobotsInc.Inspections.Models.Inspection;
using Note = RobotsInc.Inspections.Models.Note;
using Robot = RobotsInc.Inspections.Models.Robot;
using User = RobotsInc.Inspections.Models.Security.User;

namespace RobotsInc.Inspections.Host;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine($"Running {typeof(Program).Namespace}");

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        Configure(builder.Services, builder.Configuration);
        WebApplication app = builder.Build();
        Configure(app);
        app.Run();
    }

    private static void Configure(IServiceCollection services, IConfiguration configuration)
    {
        // EntityFrameworkCore
        services
            .AddDbContext<InspectionsDbContext>(
                options =>
                {
                    options
                        .UseLazyLoadingProxies()
                        .EnableDetailedErrors()
                        .EnableSensitiveDataLogging()
                        .UseSqlServer(
                            configuration.GetConnectionString("Inspections"),
                            sqlServerOptions =>
                            {
                                sqlServerOptions.MigrationsAssembly("RobotsInc.Inspections.Migrations");
                            });
                });

        // asp.net core
        services
            .AddControllers()
            .AddApplicationPart(typeof(HealthController).Assembly)
            .AddJsonOptions(
                options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.AllowTrailingCommas = false;
                    options.JsonSerializerOptions.Converters
                        .Add(new JsonStringEnumConverter(null, false));
                    options.JsonSerializerOptions.Converters
                        .Add(new RobotJsonConverter());
                });

        // api versioning
        services
            .AddApiVersioning(
                options =>
                {
                    options.ReportApiVersions = true;
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                })
            .AddVersionedApiExplorer(
                options =>
                {
                    options.GroupNameFormat = "'v'V";
                    options.SubstituteApiVersionInUrl = true;
                    options.SubstitutionFormat = "V";
                });

        // swagger
        services
            .AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>()
            .AddSwaggerGen();

        // http context
        services
            .AddHttpContextAccessor();

        // authentication & authorization
        services
            .AddAuthentication(GoogleJwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(
                GoogleJwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.UseGoogle(configuration["Authentication:Google:ClientId"]);
                    options.TokenValidationParameters.NameClaimType = ClaimTypes.Email;
                    options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;
                });
        services
            .AddAuthorization(
                options =>
                {
                    options.AddPolicy(
                        Policy.MANAGE_USER_CLAIMS.ToString(),
                        policyBuilder =>
                        {
                            policyBuilder.RequireUserName(configuration["Authentication:AdminAccount"]);
                        });
                    options.AddPolicy(
                        Policy.EDIT_INSPECTIONS.ToString(),
                        policyBuilder =>
                        {
                            policyBuilder.RequireRole(ClaimTypes.Values.RoleEmployee);
                        });
                    options.AddPolicy(
                        Policy.CONSULT_INSPECTIONS.ToString(),
                        policyBuilder =>
                        {
                            policyBuilder.AddRequirements(new ConsultRequirement());
                        });
                })
            .AddTransient<IClaimsTransformation, InspectionsClaimsTransformation>()
            .AddSingleton<IAuthorizationHandler, EmployeeConsultHandler>()
            .AddSingleton<IAuthorizationHandler, CustomerConsultHandler>();

        // own registrations
        // managers
        services.AddTransient<IHealthManager, HealthManager>();
        services.AddSingleton<ITimeProvider, TimeProvider>();
        services.AddSingleton<IOfficeHoursManager, OfficeHoursManager>();

        services.AddTransient<IUserManager, UserManager>();
        services.AddTransient<IClaimManager, ClaimManager>();

        services.AddTransient<ICustomerManager, CustomerManager>();
        services.AddTransient<IRobotManager<Robot>, RobotManager<Robot>>();
        services.AddTransient<IArticulatedRobotManager, ArticulatedRobotManager>();
        services.AddTransient<IAutomatedGuidedVehicleManager, AutomatedGuidedVehicleManager>();
        services.AddTransient<IInspectionManager, InspectionManager>();
        services.AddTransient<INoteManager, NoteManager>();
        services.AddTransient<IPhotoManager, PhotoManager>();

        // mappers
        services.AddTransient<IMapper<User, API.I.Security.User>, UserMapper>();
        services.AddTransient<IMapper<Claim, API.I.Security.Claim>, ClaimMapper>();

        services.AddTransient<IMapper<Customer, API.I.Customer>, CustomerMapper>();
        services.AddTransient<IMapper<ArticulatedRobot, API.I.ArticulatedRobot>, ArticulatedRobotMapper>();
        services.AddTransient<IMapper<AutomatedGuidedVehicle, API.I.AutomatedGuidedVehicle>, AutomatedGuidedVehicleMapper>();
        services.AddTransient<IMapper<Inspection, API.I.Inspection>, InspectionMapper>();
        services.AddTransient<IMapper<Note, API.I.Note>, NoteMapper>();

        // repositories
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IClaimRepository, ClaimRepository>();

        services.AddTransient<ICustomerRepository, CustomerRepository>();
        services.AddTransient<IRobotRepository<Robot>, RobotRepository<Robot>>();
        services.AddTransient<IArticulatedRobotRepository, ArticulatedRobotRepository>();
        services.AddTransient<IAutomatedGuidedVehicleRepository, AutomatedGuidedVehicleRepository>();
        services.AddTransient<IInspectionRepository, InspectionRepository>();
        services.AddTransient<INoteRepository, NoteRepository>();
        services.AddTransient<IPhotoRepository, PhotoRepository>();
    }

    private static void Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app
                .UseSwagger()
                .UseSwaggerUI(
                    options =>
                    {
                        options.OAuthUsePkce();
                        options.OAuthClientId(app.Configuration["Authentication:Google:ClientId"]);
                        options.OAuthClientSecret(app.Configuration["Authentication:Google:ClientSecret"]);

                        IApiVersionDescriptionProvider apiVersionDescriptionProvider =
                            app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                        foreach (ApiVersionDescription description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                        {
                            options.SwaggerEndpoint(
                                $"../swagger/{description.GroupName}/swagger.json",
                                $"RobotsInc Inspections API {description.GroupName}");
                        }
                    });
        }

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }

    public class AuthorizationOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            IList<object> actionMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
            bool isAuthorized = actionMetadata.Any(metadataItem => metadataItem is AuthorizeAttribute);
            bool allowAnonymous = actionMetadata.Any(metadataItem => metadataItem is AllowAnonymousAttribute);
            if (!isAuthorized || allowAnonymous)
            {
                return;
            }

            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Security =
                new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference =
                                    new OpenApiReference
                                    {
                                        Id = "GoogleIdToken",
                                        Type = ReferenceType.SecurityScheme
                                    }
                            },
                            new List<string> { "email" }
                        }
                    }
                };
        }
    }

    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            // settings
            options.IgnoreObsoleteActions();
            options.IgnoreObsoleteProperties();

            options.EnableAnnotations(true, true);

            // Define the OAuth2.0 scheme that's in use
            options.AddSecurityDefinition(
                "GoogleIdToken",
                new OpenApiSecurityScheme
                {
                    Description = "Google Authentication",
                    Scheme = "Bearer",
                    Type = SecuritySchemeType.OAuth2,
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Flows =
                        new OpenApiOAuthFlows
                        {
                            AuthorizationCode =
                                new OpenApiOAuthFlow
                                {
                                    AuthorizationUrl = new Uri("https://accounts.google.com/o/oauth2/v2/auth"),
                                    TokenUrl = new Uri("https://oauth2.googleapis.com/token"),
                                    Scopes =
                                        new Dictionary<string, string>
                                        {
                                            { "email", "The email of the user is used to identify the user." }
                                        }
                                }
                        },
                    Extensions =
                        new Dictionary<string, IOpenApiExtension>
                        {
                            { "x-tokenName", new OpenApiString("id_token") }
                        }
                });

            // apply the security definition on the endpoints using the operation filter
            options.OperationFilter<AuthorizationOperationFilter>();

            // display xml doc
            Assembly root = Assembly.GetExecutingAssembly();
            if (root.FullName != null)
            {
                string baseName = root.FullName.Substring(0, root.FullName.IndexOf('.'));

                // Set the comments path for the Swagger JSON and UI.
                List<string> xmlCommentFiles =
                    AppDomain
                        .CurrentDomain
                        .GetAssemblies()
                        .Select(a => a.GetName().Name)
                        .Where(
                            n =>
                                n is not null
                                && n.StartsWith(baseName, StringComparison.OrdinalIgnoreCase)
                                && (n.Contains("Server") || n.Contains("API")))
                        .Select(n => n + ".xml")
                        .ToList();
                foreach (string xmlCommentFile in xmlCommentFiles)
                {
                    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlCommentFile);
                    options.IncludeXmlComments(xmlPath);
                }
            }

            // title for different versions
            foreach (ApiVersionDescription description in _provider.ApiVersionDescriptions)
            {
                options
                    .SwaggerDoc(
                        description.GroupName,
                        new OpenApiInfo
                        {
                            Version = description.ApiVersion.ToString(),
                            Title = "RobotsInc Inspections API",
                            Description =
                                "This is the RobotsInc Inspections API specification.  This api will be used by"
                                + " both the Inspections web application and by the Android app.",
                            Contact =
                                new OpenApiContact
                                {
                                    Name = "Anna Lyst",
                                    Email = "anna.lyst@boutique.eu"
                                }
                        });
            }
        }
    }
}