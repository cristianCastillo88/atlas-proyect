namespace BackendAtlas.DTOs
{
    public class DetallePedidoCreateDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public string? Aclaraciones { get; set; }
    }
}