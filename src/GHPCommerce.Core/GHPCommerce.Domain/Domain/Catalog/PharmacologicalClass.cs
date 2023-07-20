using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Domain.Catalog
{
    public class PharmacologicalClass : AggregateRoot<Guid>
    {
        public PharmacologicalClass()
        {
            Products = new List<Product>();
        }

        public PharmacologicalClass(Guid id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
        public PharmacologicalClass( string name, string description)
        {
            Name = name;
            Description = description;
        }

        public Guid Id { get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
