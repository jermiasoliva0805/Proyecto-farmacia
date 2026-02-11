using FluentValidation;
using Back.DTOs;

public class DeliveryAttemptValidator : AbstractValidator<DeliveryAttemptDTO>
{
    public DeliveryAttemptValidator()
    {
        RuleFor(x => x.PedidoId).NotEmpty().WithMessage("El ID del pedido es obligatorio.");
        RuleFor(x => x.Comentario)
            .MaximumLength(200).WithMessage("El comentario no puede exceder los 200 caracteres.");
    }
}