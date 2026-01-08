namespace BackendAtlas.DTOs
{
    public class CategoriaDto
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public int SucursalId { get; set; }
    }
}