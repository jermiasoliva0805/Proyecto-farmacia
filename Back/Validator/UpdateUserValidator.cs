using FluentValidation;
using Back.DTOS;

namespace Back.Validators
{
    public class UpdateUserValidator : AbstractValidator<UpdateUserDTO>
    {
        public UpdateUserValidator()
        {
            // Validaciones opcionales para actualización
            // Los campos pueden venir nulos/vacíos
            
            RuleFor(x => x.Nombre)
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Nombre));

            RuleFor(x => x.Apellido)
                .MaximumLength(100).WithMessage("El apellido no puede exceder 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Apellido));

            RuleFor(x => x.UsuarioNombre)
                .Length(3, 50).WithMessage("El usuario debe tener entre 3 y 50 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.UsuarioNombre));

            RuleFor(x => x.Mail)
                .EmailAddress().WithMessage("El correo debe tener un formato válido.")
                .When(x => !string.IsNullOrEmpty(x.Mail));

            RuleFor(x => x.Contraseña)
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Contraseña));

            RuleFor(x => x.Rol)
                .Must(rol => rol == "Encargado" || rol == "Operario" || rol == "Cadete")
                .WithMessage("Rol inválido.")
                .When(x => !string.IsNullOrEmpty(x.Rol));

            RuleFor(x => x.IDSucursal)
                .GreaterThan(0).WithMessage("Sucursal inválida.")
                .When(x => x.IDSucursal.HasValue);
        }
    }
}