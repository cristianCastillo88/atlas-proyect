namespace BackendAtlas.DTOs
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CambiarPasswordDto
    {
        public string PasswordActual { get; set; } = string.Empty;
        public string PasswordNueva { get; set; } = string.Empty;
        public string ConfirmarPasswordNueva { get; set; } = string.Empty;
    }

    public class SolicitarRecuperacionDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class RestablecerConTokenDto
    {
        public string Token { get; set; } = string.Empty;
        public string NuevaPassword { get; set; } = string.Empty;
        public string ConfirmarPassword { get; set; } = string.Empty;
    }

    public class RestablecerPorAdminDto
    {
        public int UsuarioId { get; set; }
    }
}
