using System;
using AutoMapper;
using GHPCommerce.Application.Catalog.Brands.Commands;

namespace GHPCommerce.WebApi.Models.Brands
{
    public class BrandModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class BrandModelConfigurationMapping : Profile
    {
        public BrandModelConfigurationMapping()
        {
            CreateMap<CreateBrandCommand, BrandModel>().ReverseMap();
            CreateMap<UpdateBrandCommand, BrandModel>().ReverseMap();
        }
    }
}