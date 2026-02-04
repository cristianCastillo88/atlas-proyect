namespace BackendAtlas.Configuration
{
    /// <summary>
    /// Configuración de email/SMTP con seguridad reforzada.
    /// CRITICAL: NO hardcodear credenciales aquí.
    /// Usar:
    /// - User Secrets en desarrollo
    /// - Variables de entorno en producción
    /// - Azure Key Vault / AWS Secrets Manager en cloud
    /// </summary>
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587; // 587 = TLS, 465 = SSL
        public bool UseSsl { get; set; } = true;
        
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "Atlas - Sistema de Menús";
        
        /// <summary>
        /// Usuario SMTP - Nunca hardcodear, usar secrets
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Password SMTP - CRITICAL SECURITY
        /// Gmail: usar App Password, NO password de cuenta
        /// SendGrid/Mailgun: usar API Key
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
