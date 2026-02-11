using FluentValidation;
using Back.DTOS;

namespace Back.Validators
{
    public class RegisterUserValidator : AbstractValidator<RegisterDTO>
    {
        public RegisterUserValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

            RuleFor(x => x.UsuarioNombre)
                .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
                .Length(3, 50).WithMessage("El usuario debe tener entre 3 y 50 caracteres.");

            RuleFor(x => x.Mail)
                .NotEmpty().WithMessage("El correo es obligatorio.")
                .EmailAddress().WithMessage("El formato del correo es inválido.");

            RuleFor(x => x.Contraseña)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

            // Validación estricta del Rol (RF8)
            RuleFor(x => x.Rol)
                .NotEmpty().WithMessage("El rol es obligatorio.")
                .Must(rol => rol == "Administrador" || rol == "Operario" || rol == "Cadete")
                .WithMessage("El rol debe ser: Administrador, Operario o Cadete.");

            RuleFor(x => x.IDSucursal)
                .GreaterThan(0).WithMessage("Debe seleccionar una sucursal válida.");
        }
    }
}