namespace BackendAtlas.Domain
{
    public class Usuario
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required string Nombre { get; set; }
        public int? NegocioId { get; set; }
        public int? SucursalId { get; set; }
        public RolUsuario Rol { get; set; }
        public bool Activo { get; set; } = true;

        // Navegación
        public Negocio? Negocio { get; set; }
        public Sucursal? Sucursal { get; set; }
    }
}