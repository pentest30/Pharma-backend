using System;
using AutoMapper;
using GHPCommerce.Application.Catalog.Dosages.Commands;

namespace GHPCommerce.WebApi.Models.Dosages
{
    public class DosageModel
    {
        public Guid Id { get; set; }
        public string NAme { get; set; }
    }

    public class DosageModelConfigurationMapping : Profile
    {
        public DosageModelConfigurationMapping()
        {
            CreateMap<CreateDosageCommand, DosageModel>().ReverseMap();
            CreateMap<UpdateDosageCommand, DosageModel>().ReverseMap();
        }
    }
}