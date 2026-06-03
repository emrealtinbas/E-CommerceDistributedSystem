using AutoMapper;
using Catalog.Application.Products.Models;
using Catalog.Domain.Entities;

namespace Catalog.Application.Products.Mapping;

public sealed class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>();
    }
}
