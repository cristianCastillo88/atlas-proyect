namespace BackendAtlas.DTOs
{
    /// <summary>
    /// DTO de sucursal pública con información ESTÁTICA (cacheable)
    /// Los productos NO incluyen precio ni stock
    /// </summary>
    public class SucursalPublicaEstaticaDto
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Direccion { get; set; }
        public required string Telefono { get; set; }
        public required string Slug { get; set; }
        public string? Horario { get; set; }
        public string? UrlInstagram { get; set; }
        public string? UrlFacebook { get; set; }
        
        /// <summary>
        /// Productos con info estática SOLAMENTE (sin precio ni stock)
        /// </summary>
        public List<ProductoPublicoEstaticaDto> Productos { get; set; } = new();
        
        public List<MetodoPagoDto> MetodosPago { get; set; } = new();
        public List<TipoEntregaDto> TiposEntrega { get; set; } = new();
        public decimal? PrecioDelivery { get; set; }
    }
}
