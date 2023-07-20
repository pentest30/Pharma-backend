using AutoMapper;
using GHPCommerce.Modules.Inventory.DTOs.Zone;
using GHPCommerce.Modules.Inventory.Entities;

namespace GHPCommerce.Modules.Inventory.MapperProfiles
{
    public class StockZoneMappingConfig : Profile
    {
        public StockZoneMappingConfig()
        {
            CreateMap<ZoneDto, StockZone>().ReverseMap();
        }
    }
}