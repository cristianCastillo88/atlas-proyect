namespace BackendAtlas.Services.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Envía email de recuperación de contraseña con token de seguridad.
        /// </summary>
        Task EnviarEmailRecuperacionPasswordAsync(
            string emailDestino, 
            string nombreUsuario, 
            string resetToken, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Envía email genérico con HTML.
        /// </summary>
        Task EnviarEmailAsync(
            string emailDestino, 
            string asunto, 
            string cuerpoHtml, 
            CancellationToken cancellationToken = default);
    }
}
