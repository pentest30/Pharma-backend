using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Domain.Catalog
{
    public class List :AggregateRoot<Guid>
    {
       

        public List()
        {
            Products = new List<Product>();
        }

        public List( Guid id, string code, string name, decimal shp)
        {
            Id = id;
            Code = code;
            Name = name;
            SHP = shp;
        }

        public List(string code, string name, decimal shp)
        {
            Code = code;
            Name = name;
            SHP = shp;
        }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal SHP { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}