namespace BackendAtlas.Services.Interfaces
{
    public interface IPasswordPolicyService
    {
        /// <summary>
        /// Valida que la contraseña cumpla con las políticas de seguridad
        /// </summary>
        PasswordValidationResult ValidatePassword(string password);
        
        /// <summary>
        /// Genera una contraseña temporal segura
        /// </summary>
        string GenerateTemporaryPassword(int length = 12);
        
        /// <summary>
        /// Calcula la fortaleza de una contraseña (0-100)
        /// </summary>
        int CalculatePasswordStrength(string password);
    }

    public class PasswordValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
