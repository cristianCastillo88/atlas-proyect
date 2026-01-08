namespace BackendAtlas.Domain
{
    public class Categoria
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public bool Activa { get; set; }
        public int SucursalId { get; set; }

        // Navegaciones
        public Sucursal? Sucursal { get; set; }

        // Navegación inversa
        public ICollection<Producto>? Productos { get; set; }
    }
}