using System;

namespace GHPCommerce.Application.Catalog.TaxGroups.DTOs
{
    public class TaxGroupDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal TaxValue { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }

    
}