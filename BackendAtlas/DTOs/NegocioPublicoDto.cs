namespace BackendAtlas.DTOs
{
    public class NegocioPublicoDto
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Slug { get; set; }
        public string? UrlLogo { get; set; }
        public List<SucursalResumenDto> Sucursales { get; set; } = new();
    }

    public class SucursalResumenDto
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Direccion { get; set; }
        public required string Telefono { get; set; }
        public required string Slug { get; set; }
    }
}
