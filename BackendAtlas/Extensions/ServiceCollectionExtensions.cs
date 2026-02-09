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
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                                 ?? configuration["DefaultConnection"];
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
            }

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
