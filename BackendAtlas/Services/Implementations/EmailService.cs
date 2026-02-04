using Microsoft.Extensions.Options;
using BackendAtlas.Configuration;
using BackendAtlas.Services.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace BackendAtlas.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger,
            IConfiguration configuration)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task EnviarEmailRecuperacionPasswordAsync(
            string emailDestino,
            string nombreUsuario,
            string resetToken,
            CancellationToken cancellationToken)
        {
            var baseUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:4322";
            var resetUrl = $"{baseUrl}/reset-password?token={resetToken}";

            var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "Email", "PasswordReset.html");
            var templateContent = await File.ReadAllTextAsync(templatePath, cancellationToken);

            var cuerpoHtml = templateContent
                .Replace("{{NombreUsuario}}", nombreUsuario)
                .Replace("{{ResetUrl}}", resetUrl)
                .Replace("{{Year}}", DateTime.Now.Year.ToString());

            await EnviarEmailAsync(
                emailDestino,
                "Recuperación de Contraseña - Atlas",
                cuerpoHtml,
                cancellationToken);
        }

        public async Task EnviarEmailAsync(
            string emailDestino,
            string asunto,
            string cuerpoHtml,
            CancellationToken cancellationToken)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress("", emailDestino));
                message.Subject = asunto;

                message.Body = new TextPart(TextFormat.Html)
                {
                    Text = cuerpoHtml
                };

                using var client = new SmtpClient();
                // Seguridad: CheckCertificateRevocation = false solo para dev si hay problemas con certs locales (MailHog, etc)
                // En producción debería ser true por defecto
                
                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, _emailSettings.UseSsl, cancellationToken);
                
                // Solo autenticar si hay usuario y password configurados
                if (!string.IsNullOrEmpty(_emailSettings.Username) && !string.IsNullOrEmpty(_emailSettings.Password))
                {
                    await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password, cancellationToken);
                }
                
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Email enviado exitosamente a {Email}", emailDestino);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico enviando email a {Email}. Host: {Host}, Port: {Port}", 
                    emailDestino, _emailSettings.SmtpHost, _emailSettings.SmtpPort);
                
                // Relanzar excepción para que el caller sepa que falló
                throw new InvalidOperationException($"No se pudo enviar el email: {ex.Message}", ex);
            }
        }
    }
}
