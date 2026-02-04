namespace BackendAtlas.Services.Interfaces
{
    /// <summary>
    /// Servicio para generación de códigos QR
    /// Utiliza QRCoder con nivel de corrección de errores Q (25% de recuperación)
    /// </summary>
    public interface IQRCodeService
    {
        /// <summary>
        /// Genera un código QR como arreglo de bytes PNG
        /// </summary>
        /// <param name="url">URL a codificar en el QR</param>
        /// <param name="pixelsPerModule">Tamaño del QR (pixeles por módulo). Default: 20 (~500px)</param>
        /// <returns>Imagen PNG como byte array</returns>
        byte[] GenerateQRCodePng(string url, int pixelsPerModule = 20);

        /// <summary>
        /// Genera un código QR como SVG (escalable, mejor para impresión)
        /// </summary>
        /// <param name="url">URL a codificar en el QR</param>
        /// <param name="pixelsPerModule">Tamaño relativo. Default: 20</param>
        /// <returns>SVG como string</returns>
        string GenerateQRCodeSvg(string url, int pixelsPerModule = 20);
    }
}
