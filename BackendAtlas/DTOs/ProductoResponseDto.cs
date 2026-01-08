namespace BackendAtlas.DTOs
{
    public class ProductoResponseDto
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string? UrlImagen { get; set; }
        public bool Activo { get; set; }
        public int Stock { get; set; }
        public required string CategoriaNombre { get; set; }
    }
}