using System.ComponentModel.DataAnnotations;

namespace BackendAtlas.DTOs
{
    public class NegocioUpdateDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public required string Nombre { get; set; }

        [Required(ErrorMessage = "El slug es requerido")]
        [StringLength(100, ErrorMessage = "El slug no puede exceder los 100 caracteres")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "El slug solo puede contener letras minúsculas, números y guiones medios")]
        public required string Slug { get; set; }
    }
}
