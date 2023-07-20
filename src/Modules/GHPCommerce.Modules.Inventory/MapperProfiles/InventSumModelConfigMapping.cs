using System;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Modules.Inventory.Commands;
using GHPCommerce.Modules.Inventory.DTOs;
using GHPCommerce.Modules.Inventory.Entities;

namespace GHPCommerce.Modules.Inventory.MapperProfiles
{
    public class InventSumModelConfigMapping : Profile
    {
        public InventSumModelConfigMapping()
        {
            CreateMap<InventSum, CreateInventSumCommand>().ReverseMap();
            CreateMap<InventSum, CreateAXInventSumCommand>().ForMember(x=>x.PhysicalOnHandQuantity, o=> o.MapFrom(i=>i.PhysicalOnhandQuantity)).ReverseMap();
            CreateMap<InventSum, UpdateInventSumCommand>().ReverseMap();
            CreateMap<AddTransEntryReleaseInventCommand, InventItemTransaction>().ReverseMap();
            CreateMap<InventItemTransaction, InventItemTransactionDto>()
                .ForMember(x=>x.ZoneName , p=> p.MapFrom(i=> i.Invent !=null ? i.Invent.ZoneName : String.Empty))
                .ForMember(x=>x.StockStateName , p=> p.MapFrom(i=> i.Invent !=null ? i.Invent.StockStateName : String.Empty))
                .ForMember(x=>x.ZoneId , p=> p.MapFrom(i=> i.Invent !=null ? i.Invent.ZoneId : default))
                .ForMember(x=>x.StockStateId , p=> p.MapFrom(i=> i.Invent !=null ? i.Invent.StockStateId : default))
                .ReverseMap();
            CreateMap<ZoneTypeDto, ZoneType>().ReverseMap();


        }
    }
    public class InventSumCreatedEventConfigurationMapping : Profile
    {
        public InventSumCreatedEventConfigurationMapping()
        {
            CreateMap<InventSum,CachedInventSum >().ForMember(x=>x.Packing, o=> o.MapFrom(c=>c.packing)).ReverseMap();

        }
    }
}