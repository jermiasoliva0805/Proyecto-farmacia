using FluentValidation;
using Back.DTOS;

namespace Back.Validators
{
    public class UpdateUserValidator : AbstractValidator<UpdateUserDTO>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.IDUsuario)
                .GreaterThan(0).WithMessage("ID de usuario inválido.");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es obligatorio.");

            RuleFor(x => x.UsuarioNombre)
                .NotEmpty().WithMessage("El nombre de usuario es obligatorio.");

            RuleFor(x => x.Mail)
                .NotEmpty().WithMessage("El correo es obligatorio.")
                .EmailAddress();

            RuleFor(x => x.Rol)
                .NotEmpty()
                .Must(rol => rol == "Administrador" || rol == "Operario" || rol == "Cadete")
                .WithMessage("Rol inválido.");

            RuleFor(x => x.IDSucursal)
                .GreaterThan(0).WithMessage("Sucursal inválida.");
        }
    }
}