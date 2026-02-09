namespace BackendAtlas.DTOs
{
    public class TenantDto
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public string? Slug { get; set; }
        public required string DueñoEmail { get; set; }
        public int CantidadSucursales { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class ToggleTenantStatusDto
    {
        public bool Activo { get; set; }
    }
}