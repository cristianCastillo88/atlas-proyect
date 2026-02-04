namespace BackendAtlas.DTOs
{
    public class DetallePedidoAdminDto
    {
        public int ProductoId { get; set; }
        public required string ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string? Aclaraciones { get; set; }
    }
}
