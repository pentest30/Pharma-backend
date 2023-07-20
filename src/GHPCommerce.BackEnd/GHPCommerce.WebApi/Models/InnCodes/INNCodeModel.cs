using System;
using System.Collections.Generic;
using AutoMapper;
using GHPCommerce.Application.Catalog.INNCodes.Commands;
using GHPCommerce.Application.Catalog.INNCodes.DTOs;

namespace GHPCommerce.WebApi.Models.InnCodes
{
    public class INNCodeModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid FormId { get; set; }
        public List<InnCodeDosageDto> InnCodeDosages { get; set; }
    }

    public class INNCodeModelConfigurationMapping : Profile
    {
        public INNCodeModelConfigurationMapping()
        {
            CreateMap<CreateInnCodeCommand, INNCodeModel>().ReverseMap();
            CreateMap<UpdateInnCodeCommand, INNCodeModel>().ReverseMap();
        }
    }
}