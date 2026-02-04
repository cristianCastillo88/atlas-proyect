namespace BackendAtlas.DTOs
{
    public class SucursalCreateDto
    {
        public int NegocioId { get; set; }
        public required string Nombre { get; set; }
        public required string Direccion { get; set; }
        public required string Telefono { get; set; }
    }

    public class SucursalUpdateDto
    {
        public required string Direccion { get; set; }
        public required string Telefono { get; set; }
        public string? Horario { get; set; }
        public string? UrlInstagram { get; set; }
        public string? UrlFacebook { get; set; }
        public decimal PrecioDelivery { get; set; }
    }

    public class SucursalResponseDto
    {
        public int Id { get; set; }
        public int NegocioId { get; set; }
        public required string Nombre { get; set; }
        public required string Slug { get; set; }
        public required string Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Horario { get; set; }
        public string? UrlInstagram { get; set; }
        public string? UrlFacebook { get; set; }
        public decimal PrecioDelivery { get; set; }
        public bool Activo { get; set; }
    }
}