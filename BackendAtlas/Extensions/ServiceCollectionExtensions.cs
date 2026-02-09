     using BackendAtlas.Data;
using BackendAtlas.Repositories.Implementations;
using BackendAtlas.Repositories.Interfaces;
using BackendAtlas.Services.Implementations;
using BackendAtlas.Services.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using BackendAtlas.Mapping;

namespace BackendAtlas.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database Context
            // 1. Intentar obtener de múltiples fuentes comunes
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                                 ?? configuration["DefaultConnection"] 
                                 ?? configuration["DATABASE_URL"]
                                 ?? configuration["MYSQL_URL"]
                                 ?? configuration["MYSQLURL"];

            // 2. Traductor de formato mysql:// a Server=...
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
                catch { Log.Warning("No se pudo traducir el formato mysql://, se usará la original."); }
            }
            
            // 3. Fallback manual por piezas (Railway vinculación directa)
            if (string.IsNullOrEmpty(connectionString))
            {
                var host = configuration["MYSQLHOST"] ?? configuration["MYSQL_HOST"];
                var user = configuration["MYSQLUSER"] ?? configuration["MYSQL_USER"];
                if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(user))
                {
                    var port = configuration["MYSQLPORT"] ?? configuration["MYSQL_PORT"] ?? "3306";
                    var db = configuration["MYSQLDATABASE"] ?? configuration["MYSQL_DATABASE"] ?? "railway";
                    var pwd = configuration["MYSQLPASSWORD"] ?? configuration["MYSQL_PASSWORD"];
                    connectionString = $"Server={host};Port={port};Database={db};Uid={user};Pwd={pwd};";
                }
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                Log.Fatal("!!! ERROR CRÍTICO: No se detectó ninguna variable de conexión (DATABASE_URL, DefaultConnection, etc.)");
                throw new InvalidOperationException("Conexión no encontrada.");
            }

            Log.Information("Conexión detectada exitosamente. Configurando base de datos...");

            services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            // Repositories
            services.AddScoped<ICategoriaRepository, CategoriaRepository>();
            services.AddScoped<IMetodoPagoRepository, MetodoPagoRepository>();
            services.AddScoped<ITipoEntregaRepository, TipoEntregaRepository>();
            services.AddScoped<IProductoRepository, ProductoRepository>();
            services.AddScoped<IPedidoRepository, PedidoRepository>();
            services.AddScoped<ISucursalRepository, SucursalRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<INegocioRepository, NegocioRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            services.AddScoped<ICategoriaService, CategoriaService>();
            services.AddScoped<IMaestrosService, MaestrosService>();
            services.AddScoped<IProductoService, ProductoService>();
            services.AddScoped<IPedidoService, PedidoService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<ISucursalService, SucursalService>();
            services.AddScoped<INegocioService, NegocioService>();
            services.AddScoped<IQRCodeService, QRCodeService>();

            // Security & Auth Services
            services.Configure<Configuration.EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
            services.AddScoped<IPasswordPolicyService, PasswordPolicyService>();
            services.AddScoped<IEmailService, EmailService>();

            // AutoMapper
            services.AddAutoMapper(typeof(MappingProfile));

            // FluentValidation
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<Program>();

            return services;
        }

        public static IServiceCollection AddApiConfiguration(this IServiceCollection services, IConfiguration? configuration = null)
        {
            // Controllers config
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                });

            // Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // ============ CRITICAL IMPROVEMENT #4: CORS Configuration ============
            services.AddCors(options =>
            {
                // Development policy - Allow all for easier testing
                options.AddPolicy("PermitirTodo", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true) // Permitir cualquier origen pero reflejándolo en el header
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // Necesario si el cliente envía cookies/auth headers
                });

                // Production policy - Restrict to specific origins
                if (configuration != null)
                {
                    var allowedOrigins = configuration.GetValue<string>("CorsOrigins")?.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    
                    options.AddPolicy("ProductionCors", policy =>
                    {
                        if (allowedOrigins != null && allowedOrigins.Length > 0)
                        {
                            if (allowedOrigins.Contains("*"))
                            {
                                policy.SetIsOriginAllowed(_ => true)
                                      .AllowAnyMethod()
                                      .AllowAnyHeader()
                                      .AllowCredentials();
                            }
                            else
                            {
                                policy.WithOrigins(allowedOrigins)
                                      .AllowAnyMethod()
                                      .AllowAnyHeader()
                                      .AllowCredentials();
                            }
                        }
                        else
                        {
                            policy.WithOrigins("https://localhost:3000")
                                  .AllowAnyMethod()
                                  .AllowAnyHeader()
                                  .AllowCredentials();
                        }
                    });
                }
            });

            return services;
        }
    }
}
