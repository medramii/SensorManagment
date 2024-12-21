using ServerApp.API.Extensions;
using NLog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using System.Security.Policy;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Load NLog configuration for Logger Service
LogManager.Setup().LoadConfigurationFromFile(Path.Combine(Directory.GetCurrentDirectory(), "nlog.config"));

// Add essential services to the container

// Configure API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);

    options.AssumeDefaultVersionWhenUnspecified = true;

    options.ReportApiVersions = true;

    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

// Add Redis Cache
builder.Services.ConfigureRedisCachingService(builder.Configuration);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add Controllers
builder.Services.AddControllers();

// Configure API Endpoints and Swagger
builder.Services.AddEndpointsApiExplorer();

// Congigure Swagger
builder.Services.ConfigureOptions<ConfigureSwaggerGenOptions>();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// Configure the Database Context
builder.Services.ConfigureAppDbContext(builder.Configuration);

// Configure Logger Service
builder.Services.ConfigureLoggerService();

// Configure Authentication
builder.Services.ConfigureAuthentication(builder.Configuration);

// Configure Repositories
builder.Services.ConfigureRepositories();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        IReadOnlyList<ApiVersionDescription> descriptions = app.DescribeApiVersions();
        foreach (ApiVersionDescription description in descriptions)
        {
            string url = $"/swagger/{description.GroupName}/swagger.json";
            string name = description.GroupName;

            options.SwaggerEndpoint(url, name);
        }

        options.RoutePrefix = string.Empty;
    });
}
app.UseHttpsRedirection();

// Enable CORS
app.UseCors("CorsPolicy");

app.UseRouting();

// Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

app.Run();
