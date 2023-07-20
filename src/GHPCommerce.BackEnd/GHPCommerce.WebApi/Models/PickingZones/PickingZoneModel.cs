using System;
using AutoMapper;
using GHPCommerce.Application.Catalog.PickingZones.Commands;
using GHPCommerce.Core.Shared.Contracts.PickingZone.DTOs;
using GHPCommerce.Domain.Domain.Catalog;

namespace GHPCommerce.WebApi.Models.PickingZones
{
    public class PickingZoneModel
    {
        public Guid Id { get; set; }
        public Guid? ZoneGroupId { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
        public ZoneType ZoneType { get; set; }

    }
    public class PickingZoneModelConfigurationMapping : Profile
    {
        public PickingZoneModelConfigurationMapping()
        {
            CreateMap<CreatePickingZoneCommand, PickingZoneModel>().ReverseMap();
            CreateMap<PickingZone, PickingZoneDtoV1>().ReverseMap();

            CreateMap<UpdatePickingZoneCommand, PickingZoneModel>().ReverseMap();
        }
    }
}
