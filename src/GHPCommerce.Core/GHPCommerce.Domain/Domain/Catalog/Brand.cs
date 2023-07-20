using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Domain.Catalog
{
    public class Brand : AggregateRoot<Guid>
    {
        public Brand()
        {
            Products = new List<Product>();
        }

        public Brand(Guid id , string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public Brand(string name, string description)
        {
            Name = name;
            Description = description;
        }

       
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
