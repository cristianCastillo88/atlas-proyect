# 🔐 Mejoras de Seguridad y Escalabilidad - AuthService

## ✅ Mejoras Implementadas

### 1. **Patrón CQS Implementado**
- ✅ Usa `IUsuarioRepository` en lugar de `AppDbContext` directo
- ✅ Separación clara de responsabilidades
- ✅ Testeable y escalable

### 2. **Async/Await Correctamente**
- ✅ Usa `FirstOrDefaultAsync` en lugar de `FirstOrDefault`
- ✅ Todas las operaciones son asíncronas

### 3. **Validaciones de Entrada**
- ✅ Valida email y password no vacíos
- ✅ Normaliza email a lowercase (case-insensitive)
- ✅ Búsqueda case-insensitive en base de datos

### 4. **Seguridad Mejorada**
- ✅ **Timing Attack Protection**: Ejecuta BCrypt.Verify incluso cuando el usuario no existe
- ✅ **No revela información**: No indica si el email existe o si el password es incorrecto
- ✅ **UTC Time**: Usa `DateTime.UtcNow` para evitar problemas de zona horaria
- ✅ **Token único**: Cada token tiene un `jti` (JWT ID) único
- ✅ **NotBefore claim**: Token válido desde el momento de creación
- ✅ **Claims estándar**: Incluye `iat` (issued at) para auditoría

### 5. **Logging para Auditoría**
- ✅ Log de intentos de login fallidos (email incorrecto o password incorrecto)
- ✅ Log de logins exitosos con rol
- ✅ Preparado para detectar ataques de fuerza bruta

### 6. **Configuración Flexible**
- ✅ `ExpirationMinutes` configurable desde appsettings.json
- ✅ Validación de configuración JWT en constructor
- ✅ `ClockSkew` de 5 minutos para tolerancia de sincronización

### 7. **LoginResponse Mejorado**
- ✅ Incluye `Email` y `UserId` para el frontend
- ✅ Preparado para agregar `RefreshToken` en el futuro

---

## 🚀 Recomendaciones para Escalabilidad Futura

### 1. **Implementar Refresh Tokens**
```csharp
// TODO en IAuthService:
Task<LoginResponse?> RefreshToken(string refreshToken, CancellationToken cancellationToken = default);
Task RevokeToken(string token, CancellationToken cancellationToken = default);
```

**Beneficios:**
- Tokens de corta duración (15 min) para mayor seguridad
- Refresh tokens de larga duración (7-30 días) almacenados en DB
- Permite revocar sesiones activas

**Implementación:**
1. Crear tabla `RefreshTokens` con: `Id`, `UserId`, `Token`, `ExpiresAt`, `CreatedAt`, `RevokedAt`
2. Generar refresh token en `Login()` y almacenar en DB
3. Endpoint `/api/auth/refresh` para renovar access token
4. Endpoint `/api/auth/revoke` para invalidar refresh token

### 2. **Rate Limiting / Throttling**
Usar paquete `AspNetCoreRateLimit`:

```bash
dotnet add package AspNetCoreRateLimit
```

```csharp
// Program.cs
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "POST:/api/auth/login",
            Limit = 5, // 5 intentos
            Period = "1m" // por minuto
        }
    };
});
```

**Beneficios:**
- Protección contra ataques de fuerza bruta
- Límite de 5 intentos por minuto por IP
- Respuesta 429 (Too Many Requests) cuando se excede

### 3. **Bloqueo de Cuenta Temporal**
Después de X intentos fallidos:

```csharp
// Agregar a Usuario.cs
public int FailedLoginAttempts { get; set; }
public DateTime? AccountLockedUntil { get; set; }

// En AuthService
if (usuario.AccountLockedUntil.HasValue && usuario.AccountLockedUntil.Value > DateTime.UtcNow)
{
    _logger?.LogWarning("Cuenta bloqueada hasta {LockedUntil} para: {Email}", 
        usuario.AccountLockedUntil, email);
    return null;
}

// Si password incorrecto:
usuario.FailedLoginAttempts++;
if (usuario.FailedLoginAttempts >= 5)
{
    usuario.AccountLockedUntil = DateTime.UtcNow.AddMinutes(15); // Bloquear 15 min
}
await _unitOfWork.Usuarios.ActualizarAsync(usuario);
await _unitOfWork.CompleteAsync();
```

### 4. **Two-Factor Authentication (2FA)**
```csharp
// Agregar a Usuario.cs
public bool TwoFactorEnabled { get; set; }
public string? TwoFactorSecret { get; set; }

// Usar Google.Authenticator o Twilio para SMS
public interface IAuthService
{
    Task<TwoFactorResponse> SendTwoFactorCode(string email, CancellationToken ct);
    Task<LoginResponse?> VerifyTwoFactorCode(string email, string code, CancellationToken ct);
}
```

### 5. **Redis Cache para Tokens Revocados**
```csharp
// Instalar StackExchange.Redis
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});

// Middleware para validar token no revocado
public async Task<bool> IsTokenRevoked(string jti)
{
    return await _cache.GetStringAsync($"revoked_token:{jti}") != null;
}
```

**Beneficios:**
- Escalable horizontalmente
- Invalidación instantánea de tokens
- Logout global para un usuario

### 6. **Auditoría Completa de Sesiones**
Tabla `UserSessions`:
```csharp
public class UserSession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } // jti del JWT
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime LoginAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public DateTime? LogoutAt { get; set; }
}
```

**Beneficios:**
- Ver sesiones activas del usuario
- Cerrar sesión en otros dispositivos
- Análisis de patrones de acceso

### 7. **Configuración Recomendada en appsettings.json**
```json
{
  "JwtSettings": {
    "SecretKey": "tu-clave-secreta-muy-larga-y-compleja-minimo-256-bits",
    "Issuer": "https://api.atlas.com",
    "Audience": "https://app.atlas.com",
    "ExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "RateLimiting": {
    "LoginAttemptsPerMinute": 5,
    "IpWhitelist": ["127.0.0.1"] // IPs sin límite
  },
  "Security": {
    "AccountLockoutMinutes": 15,
    "MaxFailedLoginAttempts": 5,
    "RequireTwoFactor": false,
    "PasswordMinLength": 8,
    "PasswordRequireUppercase": true,
    "PasswordRequireDigit": true,
    "PasswordRequireSpecialChar": true
  }
}
```

### 8. **Middleware de Seguridad Adicionales**
```csharp
// Program.cs
app.UseHsts(); // HTTP Strict Transport Security
app.UseXContentTypeOptions(); // Prevenir MIME sniffing
app.UseReferrerPolicy(opts => opts.NoReferrer());
app.UseXXssProtection(opts => opts.EnabledWithBlockMode());
app.UseXfo(opts => opts.Deny()); // X-Frame-Options
```

### 9. **Validación de Password Fuerte**
Crear `IPasswordValidator`:
```csharp
public interface IPasswordValidator
{
    (bool IsValid, List<string> Errors) ValidatePassword(string password);
}

// En AuthService (al crear usuario)
var (isValid, errors) = _passwordValidator.ValidatePassword(password);
if (!isValid)
{
    throw new ArgumentException(string.Join(", ", errors));
}
```

### 10. **Health Checks para Monitoreo**
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddRedis("localhost:6379");

app.MapHealthChecks("/health");
```

---

## 📊 Métricas Recomendadas (Application Insights / Prometheus)

### Métricas de Autenticación:
- **login_attempts_total** (counter): Total de intentos de login
- **login_success_total** (counter): Logins exitosos
- **login_failed_total** (counter): Logins fallidos
- **login_duration_seconds** (histogram): Tiempo de respuesta de login
- **active_sessions_total** (gauge): Sesiones activas
- **account_lockouts_total** (counter): Cuentas bloqueadas

### Alertas Sugeridas:
- ⚠️ Más de 100 logins fallidos en 5 minutos → Posible ataque
- ⚠️ Tiempo de login > 2 segundos → Revisar performance
- ⚠️ Tasa de éxito de login < 70% → Problema UX o ataque

---

## 🔒 Checklist de Seguridad

### Inmediato (Implementado ✅)
- [x] Usar repositorio en lugar de DbContext directo
- [x] Async/await correctamente
- [x] Validación de entrada
- [x] Timing attack protection
- [x] No revelar información sobre existencia de usuarios
- [x] Logging de intentos de login
- [x] UTC time para tokens
- [x] Email case-insensitive

### Corto Plazo (Semanas 1-2)
- [ ] Refresh tokens
- [ ] Rate limiting en endpoint de login
- [ ] Bloqueo temporal de cuentas
- [ ] Validador de contraseñas fuertes
- [ ] Health checks

### Mediano Plazo (Mes 1)
- [ ] Two-Factor Authentication (2FA)
- [ ] Redis para tokens revocados
- [ ] Tabla de sesiones con auditoría
- [ ] Middleware de seguridad (HSTS, CSP, etc.)

### Largo Plazo (Trimestre 1)
- [ ] Single Sign-On (SSO) con OAuth2
- [ ] Integración con Identity Provider (Auth0, Azure AD)
- [ ] Biometría (WebAuthn)
- [ ] Análisis de anomalías con ML

---

## 📝 Notas de Implementación

### Email Normalización
El email se normaliza a **lowercase** tanto en el servicio como en el repositorio para garantizar búsquedas case-insensitive. Considera agregar un índice en la columna `Email`:

```sql
CREATE INDEX IX_Usuarios_Email_Lower ON Usuarios (LOWER(Email));
```

### BCrypt Cost Factor
El costo por defecto de BCrypt es **11**. Para mayor seguridad (pero más lento):
```csharp
var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12); // Más seguro
```

### Secreto JWT
**NUNCA** commitear el secreto JWT en Git. Usar:
- **Desarrollo**: User Secrets (`dotnet user-secrets set "JwtSettings:SecretKey" "valor"`)
- **Producción**: Variables de entorno o Azure Key Vault

---

## 🎯 Resumen de Beneficios

| Mejora | Escalabilidad | Seguridad | Performance |
|--------|---------------|-----------|-------------|
| **Patrón CQS** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Refresh Tokens** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| **Rate Limiting** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| **Redis Cache** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| **2FA** | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐ |
| **Logging** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ |

---

**Última actualización**: Diciembre 2024  
**Estado**: AuthService refactorizado con mejoras base implementadas ✅
