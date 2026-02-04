using FluentValidation;
using BackendAtlas.DTOs;

namespace BackendAtlas.Validators
{
    public class ProductoCreateRequestValidator : AbstractValidator<ProductoCreateRequestDto>
    {
        public ProductoCreateRequestValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del producto es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

            RuleFor(x => x.Precio)
                .GreaterThan(0).WithMessage("El precio debe ser mayor a 0.");

            RuleFor(x => x.CategoriaId)
                .GreaterThan(0).WithMessage("El ID de la categoría debe ser válido.");
        }
    }

    public class ProductoUpdateRequestValidator : AbstractValidator<ProductoUpdateRequestDto>
    {
        public ProductoUpdateRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El ID del producto es obligatorio.");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del producto es obligatorio.");

            RuleFor(x => x.Precio)
                .GreaterThan(0).WithMessage("El precio debe ser mayor a 0.");

             RuleFor(x => x.CategoriaId)
                .GreaterThan(0).WithMessage("El ID de la categoría debe ser válido.");
        }
    }
}
