using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Modules.Procurement.DTOs;
using GHPCommerce.Modules.Procurement.Entities;

namespace GHPCommerce.Modules.Procurement.MapperProfiles
{
    public class SupplierOrderMapping : Profile
    {
        public SupplierOrderMapping()
        {
            CreateMap<SupplierOrderDto, CachedOrder>()
                .ReverseMap();
            CreateMap<SupplierOrder, CachedOrder>()
                .ForMember(x => x.ExpectedShippingDate, o => o.MapFrom(t => t.ExpectedDeliveryDate))
                .ReverseMap();
            CreateMap<SupplierOrder, SupplierOrderDto>()
                .ForMember(x => x.ExpectedShippingDate, o => o.MapFrom(t => t.ExpectedDeliveryDate))

                .ForMember(x => x.SupplierOrderStatus, o => o.MapFrom(t => (int)t.OrderStatus))

                .ReverseMap();
            CreateMap<SupplierOrderItemDto, SupplierOrderItem>().ReverseMap();
            CreateMap<SupplierOrderItemDto, CachedOrderItem>().ReverseMap();
            CreateMap<SupplierOrderItem, CachedOrderItem>().ReverseMap();
            


        }
    }
}
