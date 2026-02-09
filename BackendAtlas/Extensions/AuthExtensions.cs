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

            // Obtener la clave secreta de varias fuentes posibles
            var secretKey = configuration["JwtSettings:SecretKey"] 
                          ?? configuration["JwtSettings__SecretKey"] 
                          ?? configuration["JWT_SECRET"] 
                          ?? configuration["JWT_KEY"];

            if (string.IsNullOrEmpty(secretKey))
            {
                // En producción esto es un error fatal, pero evitamos el crash inmediato
                // para que el log pueda mostrar el error.
                Serilog.Log.Error("!!! ERROR CRÍTICO: No se encontró la clave secreta JWT (JWT_SECRET). El login fallará.");
                secretKey = "ClaveTemporalDeSeguridadParaEvitarCrashDeArranque123!"; 
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
