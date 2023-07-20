using System;
using AutoMapper;
using GHPCommerce.Application.Catalog.Forms.Commands;

namespace GHPCommerce.WebApi.Models.Forms
{
    public class FormModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class FormModelConfigurationMapping : Profile
    {
        public FormModelConfigurationMapping()
        {
            CreateMap<CreateFormCommand, FormModel>().ReverseMap();
            CreateMap<UpdateFormCommand, FormModel>().ReverseMap();
        }
    }
}
