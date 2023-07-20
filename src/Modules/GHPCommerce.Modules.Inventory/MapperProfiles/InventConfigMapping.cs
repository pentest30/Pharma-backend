using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Core.Shared.Contracts.Inventory.Dtos;
using GHPCommerce.Modules.Inventory.DTOs.Invent;
using GHPCommerce.Modules.Inventory.Entities;

namespace GHPCommerce.Modules.Inventory.MapperProfiles
{
    public class InventConfigMapping : Profile
    {
        public InventConfigMapping()
        {
            CreateMap<Invent, CreateOrUpdateInventCommand>()
                .ReverseMap();
            CreateMap<Invent, InventDtoV1>()
                //.ForMember(x=>x.ExpiryDate, o=>o.MapFrom(p=>p.ExpiryDate.HasValue? p.ExpiryDate.Value.Date.ToShortDateString():""))
                .ReverseMap();
            CreateMap<Invent, InventDto>()
                .ReverseMap();
            CreateMap<Invent, InventSum>()
                .ReverseMap();
        }
    }
}