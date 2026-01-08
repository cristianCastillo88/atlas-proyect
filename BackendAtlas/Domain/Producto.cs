namespace BackendAtlas.Domain
{
    public class Producto
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string? UrlImagen { get; set; }
        public bool Activo { get; set; }
        public int Stock { get; set; } = 0;
        public int CategoriaId { get; set; }
        public int SucursalId { get; set; }

        // Navegaciones
        public Categoria? Categoria { get; set; }
        public Sucursal? Sucursal { get; set; }

        // Navegación inversa
        public ICollection<DetallePedido>? DetallesPedido { get; set; }
    }
}