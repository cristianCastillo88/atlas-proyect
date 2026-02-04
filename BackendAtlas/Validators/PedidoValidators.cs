using FluentValidation;
using BackendAtlas.DTOs;

namespace BackendAtlas.Validators
{
    /// <summary>
    /// Validador para creación de pedidos con límites estrictos y prevención XSS
    /// </summary>
    public class PedidoCreateDtoValidator : AbstractValidator<PedidoCreateDto>
    {
        public PedidoCreateDtoValidator()
        {
            // Nombre del Cliente: REQUERIDO, límite 100 caracteres, sin HTML
            RuleFor(x => x.NombreCliente)
                .NotEmpty().WithMessage("El nombre del cliente es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre del cliente no puede exceder 100 caracteres.")
                .Must(NoContenerHtmlMalicioso).WithMessage("El nombre contiene caracteres no permitidos.");

            // Dirección: OPCIONAL, límite 200 caracteres, sin HTML
            RuleFor(x => x.DireccionCliente)
                .MaximumLength(200).WithMessage("La dirección no puede exceder 200 caracteres.")
                .Must(NoContenerHtmlMalicioso!).WithMessage("La dirección contiene caracteres no permitidos.")
                .When(x => !string.IsNullOrWhiteSpace(x.DireccionCliente));

            // Teléfono: REQUERIDO, formato internacional básico
            RuleFor(x => x.TelefonoCliente)
                .NotEmpty().WithMessage("El teléfono del cliente es obligatorio.")
                .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres.")
                .Matches(@"^[\d\s\+\-\(\)]+$").WithMessage("El teléfono solo puede contener números, espacios y caracteres: + - ( )");

            // Observaciones: OPCIONAL, límite 500 caracteres, sin HTML
            RuleFor(x => x.Observaciones)
                .MaximumLength(500).WithMessage("Las observaciones no pueden exceder 500 caracteres.")
                .Must(NoContenerHtmlMalicioso!).WithMessage("Las observaciones contienen caracteres no permitidos.")
                .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));

            // IDs: Deben ser positivos
            RuleFor(x => x.MetodoPagoId)
                .GreaterThan(0).WithMessage("Debe seleccionar un método de pago válido.");

            RuleFor(x => x.TipoEntregaId)
                .GreaterThan(0).WithMessage("Debe seleccionar un tipo de entrega válido.");

            RuleFor(x => x.SucursalId)
                .GreaterThan(0).WithMessage("La sucursal es requerida.");

            // Items: Debe haber al menos un producto
            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Debe incluir al menos un producto en el pedido.")
                .Must(items => items != null && items.Count > 0).WithMessage("Debe incluir al menos un producto en el pedido.")
                .Must(items => items != null && items.Count <= 50).WithMessage("No se pueden agregar más de 50 productos diferentes por pedido.");

            // Validar cada item del pedido
            RuleForEach(x => x.Items).SetValidator(new DetallePedidoCreateDtoValidator());
        }

        /// <summary>
        /// Detecta patrones comunes de inyección HTML/JavaScript
        /// </summary>
        private bool NoContenerHtmlMalicioso(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return true;

            // Detectar tags HTML básicos que no deberían estar en texto plano
            var patronesPeligrosos = new[]
            {
                "<script", "</script>", "javascript:", "onerror=", "onload=",
                "<iframe", "<object", "<embed", "eval(", "expression("
            };

            var inputLowerCase = input.ToLowerInvariant();
            return !patronesPeligrosos.Any(patron => inputLowerCase.Contains(patron));
        }
    }

    /// <summary>
    /// Validador para items individuales del pedido
    /// </summary>
    public class DetallePedidoCreateDtoValidator : AbstractValidator<DetallePedidoCreateDto>
    {
        public DetallePedidoCreateDtoValidator()
        {
            // ProductoId: Debe ser positivo
            RuleFor(x => x.ProductoId)
                .GreaterThan(0).WithMessage("El ID del producto es inválido.");

            // Cantidad: Entre 1 y 100 unidades por producto
            RuleFor(x => x.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.")
                .LessThanOrEqualTo(100).WithMessage("No se pueden pedir más de 100 unidades del mismo producto.");

            // Aclaraciones: OPCIONAL, límite 200 caracteres, sin HTML
            RuleFor(x => x.Aclaraciones)
                .MaximumLength(200).WithMessage("Las aclaraciones no pueden exceder 200 caracteres.")
                .Must(NoContenerHtmlMalicioso!).WithMessage("Las aclaraciones contienen caracteres no permitidos.")
                .When(x => !string.IsNullOrWhiteSpace(x.Aclaraciones));
        }

        private bool NoContenerHtmlMalicioso(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return true;

            var patronesPeligrosos = new[]
            {
                "<script", "</script>", "javascript:", "onerror=", "onload=",
                "<iframe", "<object", "<embed", "eval(", "expression("
            };

            var inputLowerCase = input.ToLowerInvariant();
            return !patronesPeligrosos.Any(patron => inputLowerCase.Contains(patron));
        }
    }
}
