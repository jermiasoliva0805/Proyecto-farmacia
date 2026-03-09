using Back.DTOs;
using FluentValidation;

namespace Back.Validator
{
    public class CancelarPedidoValidator : AbstractValidator<CancelarPedidoDTO>
    {
        public CancelarPedidoValidator()
        {
            RuleFor(x => x.PedidoId)
                .GreaterThan(0)
                .WithMessage("El ID del pedido debe ser válido.");

            RuleFor(x => x.MotivoCancelacionId)
                .GreaterThan(0)
                .WithMessage("Debe seleccionar un motivo de cancelación válido.");

            RuleFor(x => x.UsuarioId)
                .NotEmpty()
                .WithMessage("El ID del usuario es requerido.");

            // Validación condicional: Si es "Falta de stock", requirerir justificación
            RuleFor(x => x.Justificacion)
                .NotEmpty()
                .When(x => x.MotivoCancelacionId == 2) // 2 = Falta de stock
                .WithMessage("Debe especificar qué productos no tienen stock.");

            // Validación condicional: Si es "Error en el pago", requiere justificación
            RuleFor(x => x.Justificacion)
                .NotEmpty()
                .When(x => x.MotivoCancelacionId == 3) // 3 = Error en pago
                .WithMessage("Debe especificar el tipo de error en el pago.");

            RuleFor(x => x.Justificacion)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Justificacion))
                .WithMessage("La justificación no puede exceder 500 caracteres.");
        }
    }
}
