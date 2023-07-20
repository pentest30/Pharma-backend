using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.PreparationOrders.DTOs;
using GHPCommerce.Core.Shared.PreparationOrder.Commands;
using GHPCommerce.Core.Shared.PreparationOrder.DTOs;
using GHPCommerce.Modules.PreparationOrder.DTOs;
using GHPCommerce.Modules.PreparationOrder.Entities;
using PreparationOrderStatus = GHPCommerce.Modules.PreparationOrder.Entities.PreparationOrderStatus;

namespace GHPCommerce.Modules.PreparationOrder.MapperProfiles
{
    public class PreparationOrderProfile : Profile
    {
        public PreparationOrderProfile()
        {
            CreateMap<CreatePreparationOrderCommand,Entities.PreparationOrder>().ReverseMap();
            CreateMap<PreparationOrderItemDtoV1, PreparationOrderItem>().ReverseMap();
            CreateMap<Entities.PreparationOrder, PreparationOrdersDto>().ReverseMap();
            CreateMap< PreparationOrdersDtoV2, Entities.PreparationOrder>().ReverseMap();
            CreateMap<PreparationOrderItem, PreparationOrderItemDto>().ReverseMap();
            CreateMap<PreparationOrderStatus, Core.Shared.Contracts.PreparationOrders.DTOs.PreparationOrderStatus>().ReverseMap();
            CreateMap<PreparationOrderDtoV6, Entities.PreparationOrder>().ReverseMap();

        }

    }
}
