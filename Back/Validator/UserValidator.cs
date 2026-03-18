using FluentValidation;
using Back.DTOs;

namespace Back.Validators
{
    public class UserValidator : AbstractValidator<UserDTO>
    {
        public UserValidator()
        {
            // Validación del nombre real
            RuleFor(x => x.NombreCompleto)
                .NotEmpty().WithMessage("El nombre es requerido para la identificación del personal.");

            // Validación del nombre de usuario (el que usan para loguearse)
            // Ref: RF7 - Autenticación
            RuleFor(x => x.Usuario)
                .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
                .MinimumLength(4).WithMessage("El nombre de usuario debe tener al menos 4 caracteres.");

            // Validación del Mail
            // Ref: Mandato Ampliado - Notificaciones automáticas (se usará para avisos internos)
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
                .EmailAddress().WithMessage("El formato del correo electrónico no es válido.");

            // Validación del Rol
            // Ref: RF11/RF12 - Especialización de roles (Encargado, Operario, Cadete)
            RuleFor(x => x.Rol)
                .NotEmpty().WithMessage("Debe asignar un rol al usuario.")
                .Must(rol => rol == "Encargado" || rol == "Operario" || rol == "Cadete")
                .WithMessage("El rol asignado no es válido para el sistema de la farmacia.");

            // Validación de la Sucursal
            // Ref: Base de Datos - Entidad Sucursal: Segmentar la operación por ubicación.
            RuleFor(x => x.NombreSucursal)
                .NotEmpty().WithMessage("El usuario debe estar vinculado a una sucursal de la Farmacia General Paz.");
        }
    }
}