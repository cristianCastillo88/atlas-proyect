namespace BackendAtlas.DTOs
{
    public class PedidoCreateDto
    {
        public required string NombreCliente { get; set; }
        public string? DireccionCliente { get; set; }
        public required string TelefonoCliente { get; set; }
        public int MetodoPagoId { get; set; }
        public int TipoEntregaId { get; set; }
        public int SucursalId { get; set; }
        public string? Observaciones { get; set; }
        public required List<DetallePedidoCreateDto> Items { get; set; }
    }
}