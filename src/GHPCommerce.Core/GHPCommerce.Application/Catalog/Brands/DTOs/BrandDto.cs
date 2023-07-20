using System;
using AutoMapper;
using GHPCommerce.Domain.Domain.Catalog;

namespace GHPCommerce.Application.Catalog.Brands.DTOs
{
    public class BrandDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class BrandDtoConfigurationMapping : Profile
    {
        public BrandDtoConfigurationMapping()
        {
            CreateMap<BrandDto, Brand>().ReverseMap();
        }
    }
}