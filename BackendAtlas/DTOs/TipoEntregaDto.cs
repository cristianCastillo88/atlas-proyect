namespace BackendAtlas.DTOs
{
    public class TipoEntregaDto
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public decimal PrecioBase { get; set; }
    }
}