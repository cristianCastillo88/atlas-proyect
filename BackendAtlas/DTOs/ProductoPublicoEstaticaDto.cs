namespace BackendAtlas.DTOs
{
    /// <summary>
    /// DTO con información ESTÁTICA de producto para catálogo público (cacheable)
    /// NO incluye precio ni stock (datos dinámicos)
    /// </summary>
    public class ProductoPublicoEstaticaDto
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }
        public string? UrlImagen { get; set; }
        public required string CategoriaNombre { get; set; }
    }

    /// <summary>
    /// DTO con datos DINÁMICOS de producto (precio y stock en tiempo real, sin cache)
    /// </summary>
    public class ProductoDinamicoDto
    {
        public int Id { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }
}
