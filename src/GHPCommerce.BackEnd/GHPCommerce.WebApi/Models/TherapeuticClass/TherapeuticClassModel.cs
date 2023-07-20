using System;
using AutoMapper;
using GHPCommerce.Application.Catalog.TherapeuticClasses.Commands;

namespace GHPCommerce.WebApi.Models.TherapeuticClass
{
    public class TherapeuticClassModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class TherapeuticClassConfigurationMapping : Profile
    {
        public TherapeuticClassConfigurationMapping()
        {
            CreateMap<CreateTherapeuticClassCommand, TherapeuticClassModel>().ReverseMap();
            CreateMap<UpdateTherapeuticClassCommand, TherapeuticClassModel>().ReverseMap();
        }
    }
}
