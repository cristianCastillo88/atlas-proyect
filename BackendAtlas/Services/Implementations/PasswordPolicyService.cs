using System.Text.RegularExpressions;
using BackendAtlas.Services.Interfaces;

namespace BackendAtlas.Services.Implementations
{
    public class PasswordPolicyService : IPasswordPolicyService
    {
        // Lista negra básica de contraseñas comunes (se puede expandir o cargar desde archivo)
        private static readonly HashSet<string> WeakPasswords = new(StringComparer.OrdinalIgnoreCase)
        {
            "123456", "password", "12345678", "qwerty", "123456789", "12345", "1234567", "111111", "123123", "admin"
        };

        public PasswordValidationResult ValidatePassword(string password)
        {
            var result = new PasswordValidationResult { IsValid = true };
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                result.IsValid = false;
                result.Errors.Add("La contraseña no puede estar vacía");
                return result;
            }

            if (password.Length < 8)
                errors.Add("La contraseña debe tener al menos 8 caracteres");

            if (!password.Any(char.IsUpper))
                errors.Add("La contraseña debe contener al menos una letra mayúscula");

            if (!password.Any(char.IsLower))
                errors.Add("La contraseña debe contener al menos una letra minúscula");

            if (!password.Any(char.IsDigit))
                errors.Add("La contraseña debe contener al menos un número");

            // Opcional: Caracteres especiales
            // if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""{}|<>]"))
            //     errors.Add("La contraseña debe contener al menos un carácter especial");

            if (WeakPasswords.Contains(password))
                errors.Add("Esta contraseña es demasiado común y no es segura");

            if (errors.Count > 0)
            {
                result.IsValid = false;
                result.Errors = errors;
            }

            return result;
        }

        public string GenerateTemporaryPassword(int length = 12)
        {
            const string upperChars = "ABCDEFGHJKLMNOPQRSTUVWXYZ"; // Excluye I, O para evitar confusión
            const string lowerChars = "abcdefghijkmnopqrstuvwxyz"; // Excluye l para evitar confusión
            const string digits = "123456789"; // Excluye 0
            const string specialChars = "!@#$%&*";

            var random = new Random();
            var passwordChars = new List<char>();

            // Asegurar al menos uno de cada tipo
            passwordChars.Add(upperChars[random.Next(upperChars.Length)]);
            passwordChars.Add(lowerChars[random.Next(lowerChars.Length)]);
            passwordChars.Add(digits[random.Next(digits.Length)]);
            passwordChars.Add(specialChars[random.Next(specialChars.Length)]);

            // Rellenar el resto
            var allChars = upperChars + lowerChars + digits + specialChars;
            for (int i = 4; i < length; i++)
            {
                passwordChars.Add(allChars[random.Next(allChars.Length)]);
            }

            // Mezclar
            return new string(passwordChars.OrderBy(x => random.Next()).ToArray());
        }

        public int CalculatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password)) return 0;

            int score = 0;

            if (password.Length >= 8) score += 20;
            if (password.Length >= 12) score += 20;
            if (password.Any(char.IsUpper)) score += 15;
            if (password.Any(char.IsLower)) score += 15;
            if (password.Any(char.IsDigit)) score += 15;
            if (Regex.IsMatch(password, @"[!@#$%^&*(),.?""{}|<>]")) score += 15;

            return Math.Min(score, 100);
        }
    }
}
