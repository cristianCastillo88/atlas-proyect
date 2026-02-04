using BackendAtlas.DTOs;
using FluentValidation;

namespace BackendAtlas.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El email no es válido");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida");
        }
    }

    public class CambiarPasswordValidator : AbstractValidator<CambiarPasswordDto>
    {
        public CambiarPasswordValidator()
        {
            RuleFor(x => x.PasswordActual)
                .NotEmpty().WithMessage("La contraseña actual es requerida");

            RuleFor(x => x.PasswordNueva)
                .NotEmpty().WithMessage("La nueva contraseña es requerida")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres");

            RuleFor(x => x.ConfirmarPasswordNueva)
                .NotEmpty().WithMessage("Debe confirmar la contraseña")
                .Equal(x => x.PasswordNueva).WithMessage("Las contraseñas no coinciden");
        }
    }

    public class SolicitarRecuperacionValidator : AbstractValidator<SolicitarRecuperacionDto>
    {
        public SolicitarRecuperacionValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El email no es válido");
        }
    }

    public class RestablecerConTokenValidator : AbstractValidator<RestablecerConTokenDto>
    {
        public RestablecerConTokenValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("El token es requerido");

            RuleFor(x => x.NuevaPassword)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres");

            RuleFor(x => x.ConfirmarPassword)
                .NotEmpty().WithMessage("Debe confirmar la contraseña")
                .Equal(x => x.NuevaPassword).WithMessage("Las contraseñas no coinciden");
        }
    }

    public class RestablecerPorAdminValidator : AbstractValidator<RestablecerPorAdminDto>
    {
        public RestablecerPorAdminValidator()
        {
            RuleFor(x => x.UsuarioId)
                .GreaterThan(0).WithMessage("ID de usuario inválido");
        }
    }
}
