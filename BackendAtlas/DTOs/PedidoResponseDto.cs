namespace BackendAtlas.DTOs
{
    public class PedidoResponseDto
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaCreacion { get; set; }
        public required string EstadoPedido { get; set; }
    }
}