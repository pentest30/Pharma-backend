using AutoMapper;
using GHPCommerce.Modules.Inventory.Commands.StockState;
using GHPCommerce.Modules.Inventory.DTOs;
using GHPCommerce.Modules.Inventory.DTOs.StockState;

namespace GHPCommerce.Modules.Inventory.MapperProfiles
{
    public class StockStateConfigMapping : Profile
    {
        public StockStateConfigMapping()
        {
            CreateMap<Entities.StockState, CreateStockStateCommand>().ReverseMap();
            CreateMap<StockStateDto, Entities.StockState> ().ReverseMap();

            CreateMap<Entities.StockState,StockStateDtoV1> ()
                .ForMember(x=>x.ZoneType, o=>o.MapFrom(z=>z.ZoneType!=null?z.ZoneType.Name:"")).ReverseMap();
        }
    }
}
