namespace BackendAtlas.Domain
{
    public class TipoEntrega
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public decimal PrecioBase { get; set; }

        // Navegación inversa
        public ICollection<Pedido>? Pedidos { get; set; }
    }
}