using Ganss.Xss;

namespace BackendAtlas.Extensions
{
    /// <summary>
    /// Extensiones para sanitización de HTML y prevención de XSS
    /// </summary>
    public static class HtmlSanitizationExtensions
    {
        private static readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

        static HtmlSanitizationExtensions()
        {
            // Configuración estricta: Solo permitir texto plano, remover TODOS los tags HTML
            _sanitizer.AllowedTags.Clear();
            _sanitizer.AllowedAttributes.Clear();
            _sanitizer.AllowedCssProperties.Clear();
            _sanitizer.AllowedSchemes.Clear();
            
            // No permitir ningún tag HTML - Solo texto plano
            _sanitizer.KeepChildNodes = true; // Mantener contenido de texto dentro de tags
        }

        /// <summary>
        /// Sanitiza una cadena removiendo todo HTML y JavaScript malicioso.
        /// Convierte HTML a texto plano seguro.
        /// </summary>
        /// <param name="input">Texto a sanitizar</param>
        /// <returns>Texto sanitizado sin HTML</returns>
        public static string? SanitizeHtml(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Sanitizar y luego trim para remover espacios extra
            var sanitized = _sanitizer.Sanitize(input);
            
            // Decodificar entidades HTML comunes que podrían quedar
            sanitized = System.Net.WebUtility.HtmlDecode(sanitized);
            
            return sanitized?.Trim();
        }

        /// <summary>
        /// Normaliza espacios en blanco múltiples a un solo espacio
        /// </summary>
        public static string? NormalizeWhitespace(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Reemplazar múltiples espacios/tabs/newlines con un solo espacio
            return System.Text.RegularExpressions.Regex.Replace(input.Trim(), @"\s+", " ");
        }
    }
}
