using Basket.Domain.Entities;

namespace Basket.Application.Abstractions.Persistence;

public interface IBasketRepository
{
    Task<CustomerBasket?> GetAsync(string customerId, CancellationToken cancellationToken);

    Task SaveAsync(CustomerBasket basket, CancellationToken cancellationToken);

    Task DeleteAsync(string customerId, CancellationToken cancellationToken);
}
