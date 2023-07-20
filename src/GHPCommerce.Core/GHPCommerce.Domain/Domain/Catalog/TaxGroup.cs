using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Domain.Catalog
{
    public class TaxGroup : AggregateRoot<Guid>
    {
        public TaxGroup()
        {
            Products = new List<Product>();
        }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal TaxValue { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public ICollection<Product> Products { get; set; }

        public TaxGroup( string code, string name, decimal taxValue, DateTime validFrom, DateTime? validTo = null)
        {
            Code = code;
            Name = name;
            TaxValue = taxValue;
            ValidFrom = validFrom.ToLocalTime();
            if (validTo != null) 
                ValidTo = validTo.Value.ToLocalTime();
            
        }
        
    }
}
