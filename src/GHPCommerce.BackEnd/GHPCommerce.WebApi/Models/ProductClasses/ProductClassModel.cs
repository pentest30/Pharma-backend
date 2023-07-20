using System;
using AutoMapper;
using GHPCommerce.Application.Catalog.ProductClasses.Commands;

namespace GHPCommerce.WebApi.Models.ProductClasses
{
    public class ProductClassModel
    {
       
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? ParentProductClassId { get; set; }
        public bool IsMedicamentClass { get; set; }
        public string Code { get; set; }
        public Guid Id { get; set; }

    }

    
    public class ProductClassModelMappingConfiguration : Profile
    {
        public ProductClassModelMappingConfiguration()
        {
            CreateMap<CreateProductClassCommand, ProductClassModel>().ReverseMap();
            CreateMap<UpdateProductClassCommand, ProductClassModel>().ReverseMap();
        }
    }
}
