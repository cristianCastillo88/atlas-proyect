namespace BackendAtlas.DTOs
{
    public class CrearNegocioDto
    {
        public required string NombreNegocio { get; set; }
        public string DireccionCentral { get; set; } = string.Empty;
        public string DireccionSucursalPrincipal { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public required DatosDuenoDto DatosDueno { get; set; }
    }

    public class DatosDuenoDto
    {
        public required string Nombre { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}