namespace BackendAtlas.DTOs
{
    public class ProductoUpdateRequestDto
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string? UrlImagen { get; set; }
        public int Stock { get; set; }
        public int CategoriaId { get; set; }
        public int SucursalId { get; set; }
    }
}