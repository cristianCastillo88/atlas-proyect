namespace BackendAtlas.Domain
{
    public class Sucursal
    {
        public int Id { get; set; }
        public int NegocioId { get; set; }
        public required string Nombre { get; set; }
        public required string Direccion { get; set; }
        public required string Telefono { get; set; }
        public required string Slug { get; set; }
        public string? Horario { get; set; }
        public bool Activo { get; set; } = true;

        // Navegaciones
        public Negocio? Negocio { get; set; }

        // Navegaciones inversas
        public ICollection<Categoria>? Categorias { get; set; }
        public ICollection<Producto>? Productos { get; set; }
        public ICollection<Pedido>? Pedidos { get; set; }
    }
}