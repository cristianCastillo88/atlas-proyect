namespace BackendAtlas.DTOs
{
    public class SucursalPublicaDto
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Direccion { get; set; }
        public required string Telefono { get; set; }
        public required string Slug { get; set; }
        public string? Horario { get; set; }
        public string? UrlInstagram { get; set; }
        public string? UrlFacebook { get; set; }
        public List<ProductoPublicoDto> Productos { get; set; } = new();
        public List<MetodoPagoDto> MetodosPago { get; set; } = new();
        public List<TipoEntregaDto> TiposEntrega { get; set; } = new();
        public decimal? PrecioDelivery { get; set; }
    }

    public class ProductoPublicoDto
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string? UrlImagen { get; set; }
        public int Stock { get; set; }
        public required string CategoriaNombre { get; set; }
    }
}
