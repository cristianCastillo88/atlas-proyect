namespace BackendAtlas.DTOs
{
    public class MetodoPagoDto
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public string? Descripcion { get; set; }
    }
}