using System.ComponentModel.DataAnnotations;

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
        [Required(ErrorMessage = "El nombre es requerido")]
        public required string Nombre { get; set; }

        [Required(ErrorMessage = "El slug es requerido")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "El slug solo puede contener letras minúsculas, números y guiones medios")]
        public required string Slug { get; set; }

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