using AutoMapper;
using GHPCommerce.Application.Catalog.ZoneGroups.Commands;
using GHPCommerce.Application.Catalog.ZoneGroups.DTOs;
using GHPCommerce.Domain.Domain.Catalog;
using System;

namespace GHPCommerce.WebApi.Models.ZoneGroups
{
    public class ZoneGroupModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

    }
    public class ZoneGroupModelConfigurationMapping : Profile
    {
        public ZoneGroupModelConfigurationMapping()
        {
            CreateMap<CreateZoneGroupCommand, ZoneGroup>().ReverseMap();
            CreateMap<UpdateZoneGroupCommand, ZoneGroup>().ReverseMap();

            CreateMap<ZoneGroup, ZoneGroupDto>().ReverseMap();
            CreateMap<ZoneGroup, GHPCommerce.Core.Shared.Contracts.ZoneGroup.DTOs.ZoneGroupDto>().ReverseMap();

        }
    }
   
}
