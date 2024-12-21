using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServerApp.Contracts.Logging;
using ServerApp.Logging;
using ServerApp.Domain.Data;
using System.Text;
using ServerApp.Contracts.Repositories;
using ServerApp.Persistence.Repositories;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ServerApp.API.Extensions;
public static class ServiceExtensions
{
    public static void ConfigureLoggerService(this IServiceCollection services)
    {
        services.AddSingleton<ILoggerManager, LoggerManager>();
    }

    public static void ConfigureAppDbContext(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<SqlServerDbContext>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
        services.AddDbContext<PostgresDbContext>(options => options.UseNpgsql(config.GetConnectionString("PostgresConnection")));
    }

    public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]))
            };

            options.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync("{\"message\": \"Unauthorized: You should authenticate in order to access this endpoint!\"}");
                }
            };
        });
    }

    public static void ConfigureRedisCachingService(this IServiceCollection services, IConfiguration config)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = config["CacheSettings:ConnectionString"];
            options.InstanceName = config["CacheSettings:Prefix"];
        });
    }

    public static void ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();

        services.AddScoped<ICapteurRepository<SqlServerDbContext>, CapteurRepository<SqlServerDbContext>>();
        services.AddScoped<ICapteurRepository<PostgresDbContext>, CapteurRepository<PostgresDbContext>>();
    }
}

public class ConfigureSwaggerGenOptions : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerGenOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        // COnfigure api documentaion
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);

        // Configure authentication with JWT for swagger
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your token in the text input below. \n PS: To get your JWT Token ensure to authenticate via the auth endpoint!"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });

        // Configure ApiVersioning for swagger
        foreach (ApiVersionDescription description in _provider.ApiVersionDescriptions)
        {
            var openApiInfo = new OpenApiInfo
            {
                Title = $"ENERSOFT.Api v{description.ApiVersion}",
                Version = description.ApiVersion.ToString(),
                Description = description.ApiVersion.MajorVersion switch
                {
                    1 => "This version interacts with an SQL Server database.",
                    2 => "This version interacts with a PostgeSQL database.",
                    _ => ""
                }
            };

            options.SwaggerDoc(description.GroupName, openApiInfo);
        }
    }

    public void Configure(string? name, SwaggerGenOptions options)
    {
        Configure(options);
    }
}