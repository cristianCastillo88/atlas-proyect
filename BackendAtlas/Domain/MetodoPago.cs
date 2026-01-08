namespace BackendAtlas.Domain
{
    public class MetodoPago
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }
        public bool EsActivo { get; set; }

        // Navegación inversa
        public ICollection<Pedido>? Pedidos { get; set; }
    }
}