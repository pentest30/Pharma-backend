using System;
using AutoMapper;
using GHPCommerce.Application.Catalog.PharmacologicalClasses.Commands;

namespace GHPCommerce.WebApi.Models.PharmacologicalClasses
{
    public class PharmacologicalClassModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class PharmacologicalClassModelConfigurationMapping : Profile
    {
        public PharmacologicalClassModelConfigurationMapping()
        {
            CreateMap<CreatePharmacologicalClassCommand, PharmacologicalClassModel> ().ReverseMap();
            CreateMap<UpdatePharmacologicalClassCommand, PharmacologicalClassModel>().ReverseMap();
        }
    }
}
