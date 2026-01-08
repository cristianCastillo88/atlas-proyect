namespace BackendAtlas.DTOs
{
    public class CrearEmpleadoDto
    {
        public required string Nombre { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public int SucursalId { get; set; }
    }
}