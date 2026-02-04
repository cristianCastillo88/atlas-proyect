using BackendAtlas.Services.Interfaces;
using QRCoder;

namespace BackendAtlas.Services.Implementations
{
    /// <summary>
    /// Implementación profesional de generación de QR usando QRCoder
    /// - Nivel de corrección de errores: Q (25% de recuperación)
    /// - Soporta PNG y SVG
    /// - Thread-safe y optimizado para rendimiento
    /// </summary>
    public class QRCodeService : IQRCodeService
    {
        private readonly ILogger<QRCodeService> _logger;

        public QRCodeService(ILogger<QRCodeService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Genera QR en formato PNG
        /// Nivel de corrección Q permite hasta 25% de daño en el código
        /// Ideal para impresión en papel o superficies que pueden ensuciarse
        /// </summary>
        public byte[] GenerateQRCodePng(string url, int pixelsPerModule = 20)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL no puede estar vacía", nameof(url));
            }

            if (pixelsPerModule < 1 || pixelsPerModule > 100)
            {
                throw new ArgumentException("pixelsPerModule debe estar entre 1 y 100", nameof(pixelsPerModule));
            }

            try
            {
                _logger.LogDebug("Generando QR PNG para URL: {Url}, Tamaño: {Size}px", url, pixelsPerModule);

                using var qrGenerator = new QRCodeGenerator();
                
                // ECC Level Q = 25% de corrección de errores
                // Ideal para QR codes que pueden dañarse o ensuciarse
                using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                
                using var qrCode = new PngByteQRCode(qrCodeData);
                
                // darkColorHtmlHex, lightColorHtmlHex
                var pngBytes = qrCode.GetGraphic(pixelsPerModule);

                _logger.LogDebug("QR generado exitosamente. Tamaño: {Size} bytes", pngBytes.Length);

                return pngBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando QR PNG para URL: {Url}", url);
                throw;
            }
        }

        /// <summary>
        /// Genera QR en formato SVG (escalable)
        /// Mejor opción para diseñadores que necesitan editar o escalar sin pérdida de calidad
        /// </summary>
        public string GenerateQRCodeSvg(string url, int pixelsPerModule = 20)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL no puede estar vacía", nameof(url));
            }

            if (pixelsPerModule < 1 || pixelsPerModule > 100)
            {
                throw new ArgumentException("pixelsPerModule debe estar entre 1 y 100", nameof(pixelsPerModule));
            }

            try
            {
                _logger.LogDebug("Generando QR SVG para URL: {Url}", url);

                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new SvgQRCode(qrCodeData);
                
                // GetGraphic con tamaño relativo
                var svgString = qrCode.GetGraphic(pixelsPerModule);

                _logger.LogDebug("QR SVG generado exitosamente");

                return svgString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando QR SVG para URL: {Url}", url);
                throw;
            }
        }
    }
}
