using System;

namespace GHPCommerce.Application.Catalog.ProductClasses.DTOs
{
    public class ProductClassDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? ParentProductClassId { get; set; }
        public bool IsMedicamentClass { get; set; }

       
    }

  
}