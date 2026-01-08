namespace BackendAtlas.Domain
{
    public class EstadoPedido
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }

        // Navegación inversa
        public ICollection<Pedido>? Pedidos { get; set; }
    }
}