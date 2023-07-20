using System;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;

namespace GHPCommerce.Ecommerce.Models
{
    public class CatalogModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
    public class CatalogModelConfigMapping : Profile
    {
        public CatalogModelConfigMapping()
        {
            CreateMap<CatalogModel, CatalogDto>().ReverseMap();
        }
    }
}
