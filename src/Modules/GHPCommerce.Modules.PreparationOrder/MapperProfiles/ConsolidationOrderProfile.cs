using AutoMapper;
using GHPCommerce.Modules.PreparationOrder.Commands;
using GHPCommerce.Modules.PreparationOrder.Commands.Consolidation;
using GHPCommerce.Modules.PreparationOrder.DTOs;
using GHPCommerce.Modules.PreparationOrder.Entities;

namespace GHPCommerce.Modules.PreparationOrder.MapperProfiles
{
    public class ConsolidationOrderProfile : Profile
    {
        public ConsolidationOrderProfile()
        {
            CreateMap<ConsolidationCommand, ConsolidationOrder>().ReverseMap();
            CreateMap<ConsolidationUpdateCommand, ConsolidationOrder>().ReverseMap();
            CreateMap<ConsolidationOrder, ConsolidationOrdersDto>().ReverseMap();
            CreateMap<ConsolidationOrder, ConsolidationUpdateCommand>().ReverseMap();


        }

    }
}
