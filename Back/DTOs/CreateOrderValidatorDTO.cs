using FluentValidation;
namespace Back.DTOs
{
    public class CreateOrderValidator : AbstractValidator<CreateOrderDTO>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.IDCliente).GreaterThan(0);
        RuleFor(x => x.IDSucursal).GreaterThan(0);
        RuleFor(x => x.IDUsuario).GreaterThan(0);
        RuleFor(x => x.FormaDePago).NotEmpty();
        RuleFor(x => x.Detalles).NotEmpty().WithMessage("Al menos un producto debe ser agregado");
    }
}

}
