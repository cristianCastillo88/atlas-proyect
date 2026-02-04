namespace BackendAtlas.DTOs
{
    public class PedidoAdminListDto
    {
        public int Id { get; set; }
        public DateTime FechaCreacion { get; set; }
        public required string ClienteNombre { get; set; }
        public required string ClienteTelefono { get; set; }
        public string? DireccionCliente { get; set; }
        public decimal Total { get; set; }
        public required string EstadoPedidoNombre { get; set; }
        public required string TipoEntregaNombre { get; set; }
        public required string MetodoPagoNombre { get; set; }
        public required string ResumenItems { get; set; }
        public List<DetallePedidoAdminDto> Items { get; set; } = new();
    }
}