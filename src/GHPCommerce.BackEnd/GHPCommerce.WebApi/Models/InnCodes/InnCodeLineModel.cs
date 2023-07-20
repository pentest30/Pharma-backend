using System;
using AutoMapper;
using GHPCommerce.Application.Catalog.INNCodes.Commands;
using GHPCommerce.Application.Catalog.INNCodes.DTOs;

namespace GHPCommerce.WebApi.Models.InnCodes
{
    public  class InnCodeLineModel
    {
        public Guid InnCodeId { get; set; }
        public InnCodeDosageDto InnCodeDosageDto { get; set; }
    }

    public class InnCodeLineModelConfigurationMapping : Profile
    {
        public InnCodeLineModelConfigurationMapping()
        {
            CreateMap<CreateInnCodeLineCommand, InnCodeLineModel>().ReverseMap();
        }
    }
}
