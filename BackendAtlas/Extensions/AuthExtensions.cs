using BackendAtlas.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BackendAtlas.Extensions
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettingsSection = configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSettingsSection);
            var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

            // 1. Buscar en variables de entorno puras primero (Lo más seguro en Railway)
            var secretKey = configuration["JWT_SECRET"] 
                          ?? configuration["JWT_KEY"]
                          ?? configuration["JwtSettings__SecretKey"];

            // 2. Si no está o es vacío, buscar en la sección de configuración
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                secretKey = jwtSettings?.SecretKey;
            }

            // 3. Fallback final si todo lo anterior es vacío o nulo
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                Serilog.Log.Error("!!! ERROR CRÍTICO: No se encontró JWT_SECRET. Usando clave de emergencia para permitir el arranque.");
                secretKey = "EstaEsUnaClaveDeEmergenciaMuyLargaParaQueElSistemaNoFalle123!";
            }

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
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };

                // Configuración para SignalR: Leer token del QueryString
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/pedidos"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }
    }
}
