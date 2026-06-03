using FluentValidation;

namespace Catalog.Application.Products.DeactivateProduct;

public sealed class DeactivateProductCommandValidator : AbstractValidator<DeactivateProductCommand>
{
    public DeactivateProductCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.RowVersion).NotEmpty().Must(BeBase64).WithMessage("RowVersion must be base64 encoded.");
    }

    private static bool BeBase64(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        Span<byte> buffer = stackalloc byte[value.Length];
        return Convert.TryFromBase64String(value, buffer, out _);
    }
}
