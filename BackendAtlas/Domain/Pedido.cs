namespace BackendAtlas.Domain
{
    public class Pedido
    {
        public int Id { get; set; }
        public DateTime FechaCreacion { get; set; }
        public decimal Total { get; set; }
        public int EstadoPedidoId { get; set; }
        public int TipoEntregaId { get; set; }
        public int MetodoPagoId { get; set; }
        public required string NombreCliente { get; set; }
        public required string TelefonoCliente { get; set; }
        public string? DireccionCliente { get; set; }
        public string? Observaciones { get; set; }
        public int SucursalId { get; set; }

        // Navegaciones
        public EstadoPedido? EstadoPedido { get; set; }
        public TipoEntrega? TipoEntrega { get; set; }
        public MetodoPago? MetodoPago { get; set; }
        public Sucursal? Sucursal { get; set; }

        // Navegación inversa
        public ICollection<DetallePedido>? DetallesPedido { get; set; }
    }
}