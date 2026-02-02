using AutoMapper;
using DeveloperStore.Application.Contracts;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.ValueObjects;

namespace DeveloperStore.Application.Sales;

public sealed class SaleMappingProfile : Profile
{
    public SaleMappingProfile()
    {
        CreateMap<ExternalIdentity, ExternalIdentityDto>();
        CreateMap<SaleItem, SaleItemDto>()
            .ForCtorParam(nameof(SaleItemDto.UnitPrice), opt => opt.MapFrom(src => src.UnitPrice.Amount))
            .ForCtorParam(nameof(SaleItemDto.DiscountRate), opt => opt.MapFrom(src => src.Discount.Rate))
            .ForCtorParam(nameof(SaleItemDto.DiscountAmount), opt => opt.MapFrom(src => src.DiscountAmount.Amount))
            .ForCtorParam(nameof(SaleItemDto.TotalAmount), opt => opt.MapFrom(src => src.TotalAmount.Amount));

        CreateMap<Sale, SaleDto>()
            .ForCtorParam(nameof(SaleDto.TotalAmount), opt => opt.MapFrom(src => src.TotalAmount.Amount));
    }
}
