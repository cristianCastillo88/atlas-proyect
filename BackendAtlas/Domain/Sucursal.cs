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
        public string? UrlInstagram { get; set; }
        public string? UrlFacebook { get; set; }
        public bool Activo { get; set; } = true;

        // Configuración Específica
        public decimal PrecioDelivery { get; set; } = 0m;

        // Navegaciones
        public Negocio? Negocio { get; set; }

        // Navegaciones inversas
        public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}