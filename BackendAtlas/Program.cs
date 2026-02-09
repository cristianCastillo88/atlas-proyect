using System;
using BackendAtlas.Extensions;
using BackendAtlas.Middleware;
using BackendAtlas.Data;
using Serilog;
using Asp.Versioning;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables() // ¡CRUCIAL para Railway!
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "BackendAtlas")
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting BackendAtlas API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services
        .AddApiConfiguration(builder.Configuration)
        .AddApplicationServices(builder.Configuration)
        .AddJwtAuthentication(builder.Configuration);

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                         ?? builder.Configuration["DefaultConnection"] 
                         ?? builder.Configuration["DATABASE_URL"]
                         ?? builder.Configuration["MYSQL_URL"]
                         ?? builder.Configuration["MYSQLURL"];

    // Traductor universal de MySQL URL a formato EF Core
    if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("mysql://", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var uri = new Uri(connectionString);
            var db = uri.AbsolutePath.TrimStart('/');
            var userPass = uri.UserInfo.Split(':');
            var user = userPass[0];
            var pass = userPass.Length > 1 ? userPass[1] : "";
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 3306;
            connectionString = $"Server={host};Port={port};Database={db};Uid={user};Pwd={pass};SSL Mode=None;";
        }
        catch { Log.Warning("No se pudo traducir el formato mysql:// en Program.cs"); }
    }

    // Fallback absoluto para Railway
    if (string.IsNullOrEmpty(connectionString))
    {
        var host = builder.Configuration["MYSQLHOST"] ?? builder.Configuration["MYSQL_HOST"];
        var user = builder.Configuration["MYSQLUSER"] ?? builder.Configuration["MYSQL_USER"];
        if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(user))
        {
            var port = builder.Configuration["MYSQLPORT"] ?? builder.Configuration["MYSQL_PORT"] ?? "3306";
            var db = builder.Configuration["MYSQLDATABASE"] ?? builder.Configuration["MYSQL_DATABASE"] ?? "railway";
            var pwd = builder.Configuration["MYSQLPASSWORD"] ?? builder.Configuration["MYSQL_PASSWORD"];
            connectionString = $"Server={host};Port={port};Database={db};Uid={user};Pwd={pwd};";
        }
    }

    builder.Services.AddHealthChecks()
        .AddMySql(
            connectionString ?? "",
            name: "database",
            tags: new[] { "db", "sql", "mysql" })
        .AddCheck("api", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"));

    builder.Services.AddRateLimiter(options =>
    {
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

    builder.Services.AddResponseCaching();
    builder.Services.AddMemoryCache();

    builder.Services.AddSignalR();

    var app = builder.Build();

    // ============ SEEDING DE BASE DE DATOS ============
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            var configuration = services.GetRequiredService<IConfiguration>();
            DbInitializer.Initialize(context, configuration);
            Log.Information("Base de datos inicializada/verificada correctamente.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error al inicializar la base de datos.");
        }
    }

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

    app.UseRateLimiter();

    app.UseResponseCaching();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks("/health");
    
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

    app.MapControllers().RequireRateLimiting("fixed");
    
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

public partial class Program { }
