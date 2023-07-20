using System;
using AutoMapper;
using GHPCommerce.Application.Catalog.INNs.Commands;

namespace GHPCommerce.WebApi.Models.INNs
{
    public class INNModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class INNModelConfigurationMapping : Profile
    {
        public INNModelConfigurationMapping()
        {
            CreateMap<CreateInnCommand, INNModel>().ReverseMap();
            CreateMap<UpdateInnCommand, INNModel>().ReverseMap();
        }
    }
}