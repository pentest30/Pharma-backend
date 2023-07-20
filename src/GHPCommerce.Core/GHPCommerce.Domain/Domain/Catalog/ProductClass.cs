using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Domain.Catalog
{
    public class ProductClass : AggregateRoot<Guid>
    {
        public ProductClass()
        {
            Products =new List<Product>();
        }

        public ProductClass(string name, string description, Guid? parentProductClassId)
        {
            Name = name.ToUpper();
            Description = description;
            ParentProductClassId = parentProductClassId;
            Products = new List<Product>();
            IsMedicamentClass = false;
            Id = Guid.NewGuid();
        }
        public ProductClass(Guid id, string name, string description, Guid? parentProductClassId, bool isMedicamentClass)
        {
            Name = name.ToUpper();
            Description = description;
            ParentProductClassId = parentProductClassId;
            IsMedicamentClass = isMedicamentClass;
            Products = new List<Product>();
            Id = id;
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? ParentProductClassId { get; set; }
        public bool IsMedicamentClass { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
