namespace BackendAtlas.Domain
{
    public class Negocio
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public string? Slug { get; set; }
        public string? UrlLogo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; } = true;

        // Navegación inversa
        public ICollection<Sucursal> Sucursales { get; set; } = new List<Sucursal>();
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}