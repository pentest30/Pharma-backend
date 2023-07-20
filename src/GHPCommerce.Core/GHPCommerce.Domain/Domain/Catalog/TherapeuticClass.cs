using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Domain.Catalog
{
    public class TherapeuticClass : AggregateRoot<Guid>
    {
        public TherapeuticClass()
        {
            Products = new List<Product>();
        }

        public TherapeuticClass(Guid id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
        public TherapeuticClass( string name, string description)
        {
           
            Name = name;
            Description = description;
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}