using BackendAtlas.Extensions;
using BackendAtlas.Middleware;
using BackendAtlas.Data;
using Serilog;
using Asp.Versioning;

// ============ CRITICAL IMPROVEMENT #3: Structured Logging with Serilog ============
// Configure Serilog BEFORE building the host for early initialization logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "BackendAtlas")
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
    .CreateLogger();

try
{
    Log.Information("Starting BackendAtlas API...");

    var builder = WebApplication.CreateBuilder(args);

    // Replace default logging with Serilog
    builder.Host.UseSerilog();

    // Add services using Extension Methods (Clean Architecture & Separation of Concerns)
    builder.Services
        .AddApiConfiguration(builder.Configuration)
        .AddApplicationServices(builder.Configuration)
        .AddJwtAuthentication(builder.Configuration);

    // ============ CRITICAL IMPROVEMENT #2: Health Checks ============
    builder.Services.AddHealthChecks()
        .AddMySql(
            builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "database",
            tags: new[] { "db", "sql", "mysql" })
        .AddCheck("api", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"));

    // ============ HIGH PRIORITY #1: Rate Limiting ============
    builder.Services.AddRateLimiter(options =>
    {
        // Development: Sin límites para facilitar testing
        // Production: 100 requests/minuto por IP
        options.AddPolicy("fixed", context =>
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ipAddress,
                factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(1),
                    PermitLimit = builder.Environment.IsDevelopment() ? 9999 : 100,
                    QueueLimit = 0,
                    QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst
                });
        });

        // Seguridad: Límites estrictos para endpoints de autenticación (Login, Reset)
        // 5 intentos por minuto por IP para prevenir fuerza bruta
        options.AddPolicy("AuthLimit", context =>
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ipAddress,
                factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(1),
                    PermitLimit = builder.Environment.IsDevelopment() ? 100 : 5,
                    QueueLimit = 0
                });
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    // ============ HIGH PRIORITY #5: API Versioning ============
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new QueryStringApiVersionReader("api-version"),
            new HeaderApiVersionReader("X-Version")
        );
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // ============ HIGH PRIORITY #3: Response Caching ============
    builder.Services.AddResponseCaching();
    builder.Services.AddMemoryCache();

    // ============ HIGH PRIORITY #6: Real-Time Notifications (SignalR) ============
    builder.Services.AddSignalR();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseMiddleware<ExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        // En producción redirigir HTTP a HTTPS
        app.UseHttpsRedirection();
    }

    // ============ CRITICAL IMPROVEMENT #4: Secure CORS Configuration ============
    // Development: Allow all for easier testing
    // Production: Restrict to specific origins from configuration
    if (app.Environment.IsDevelopment())
    {
        app.UseCors("PermitirTodo");
        Log.Information("CORS: Development mode - Allowing all origins");
    }
    else
    {
        app.UseCors("ProductionCors");
        Log.Information("CORS: Production mode - Restricted origins only");
    }

    // ============ HIGH PRIORITY #1: Rate Limiting Middleware ============
    app.UseRateLimiter();

    // ============ HIGH PRIORITY #3: Response Caching Middleware ============
    app.UseResponseCaching();

    // Auth Pipeline
    app.UseAuthentication();
    app.UseAuthorization();

    // ============ CRITICAL IMPROVEMENT #2: Health Check Endpoints ============
    // Basic health check for load balancers
    app.MapHealthChecks("/health");
    
    // Detailed health check with component status
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            });
            await context.Response.WriteAsync(result);
        }
    });

    // Endpoint Mapping
    app.MapControllers().RequireRateLimiting("fixed");
    
    // SignalR Hubs
    app.MapHub<BackendAtlas.Hubs.PedidosHub>("/hubs/pedidos").RequireAuthorization();

    Log.Information("BackendAtlas API started successfully on {Environment}", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "BackendAtlas API failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Make the implicit Program class public so test projects can access it
public partial class Program { }
