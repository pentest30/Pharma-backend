using System;
using GHPCommerce.Domain.Domain.Catalog;

namespace GHPCommerce.Application.Catalog.Products.DTOs
{
    public class ProductDtoFromCode
    { 
        public Guid Id { get; set; }
        public string Code { get; set; }  
        public bool Quota { get; set; }
        public ProductState State { get; set; }
    }
}
