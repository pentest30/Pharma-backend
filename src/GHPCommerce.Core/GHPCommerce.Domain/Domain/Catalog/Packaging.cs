using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Domain.Catalog
{
    public class Packaging:AggregateRoot<Guid>
    {
        public Packaging()
        {
            Products = new List<Product>();
        }

        public Packaging(Guid id ,string code, string name)
        {
            Code = code;
            Name = name;
            Id = id;
        }
        public Packaging( string code, string name)
        {
            Code = code;
            Name = name;
           
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
