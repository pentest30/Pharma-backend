using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.DeliveryOrder.Dtos;
using GHPCommerce.Modules.PreparationOrder.DTOs.DeleiveryOrders;
using GHPCommerce.Modules.PreparationOrder.Entities;

namespace GHPCommerce.Modules.PreparationOrder.MapperProfiles
{
    public class DeleiveryOrderProfile: Profile
    {
        public DeleiveryOrderProfile()
        {
            CreateMap<DeleiveryOrder, DeleiveryOrderDto>().ReverseMap();
            CreateMap<PreparationOrderItem, DeleiveryOrderItem>().ReverseMap();
            CreateMap<DeleiveryOrderItem, DeleiveryOrderItemDto>().ReverseMap();
            CreateMap<DeleiveryOrder, DeliveryOrderDtoV2>().ReverseMap();
            CreateMap<DeleiveryOrderItem, DeliveryOrderItemDtoV1>().ReverseMap();
            CreateMap<PreparationOrderItem, GHPCommerce.Core.Shared.Events.PreparationOrder.PreparationOrderItem>().ReverseMap();

            
        }

    }
}