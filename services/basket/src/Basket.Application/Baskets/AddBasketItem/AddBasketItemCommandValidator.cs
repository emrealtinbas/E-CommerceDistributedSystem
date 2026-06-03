using FluentValidation;

namespace Basket.Application.Baskets.AddBasketItem;

public sealed class AddBasketItemCommandValidator : AbstractValidator<AddBasketItemCommand>
{
    public AddBasketItemCommandValidator()
    {
        RuleFor(command => command.CustomerId).NotEmpty();
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(command => command.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(command => command.Currency).NotEmpty().Length(3);
        RuleFor(command => command.Quantity).GreaterThan(0);
    }
}
