using System;
using System.Collections.Generic;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;

namespace GHPCommerce.Ecommerce.Models
{
    public class ProductModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public decimal UnitPrice { get; set; }
        public double Discount { get; set; }
        public double Tax { get; set; }
        public IEnumerable<string> Images { get; set; }
        public bool HasQuota { get; set; }

    }
    public class ProductModelConfigMapping : Profile
    {
        public ProductModelConfigMapping()
        {
            CreateMap<ProductModel, ProductDtoV3>().ReverseMap();
        }
    }
}
