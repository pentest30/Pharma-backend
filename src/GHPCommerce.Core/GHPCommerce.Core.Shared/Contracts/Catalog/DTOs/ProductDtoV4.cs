using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.DTOs
{
    [Serializable]
    public class ProductDtoV4
    {
        public Guid Id { get; set; }
        public Guid? InnCodeId { get; set; }
        public decimal TaxGroup { get; set; }
        public decimal UnitPrice { get; set; }
        public List<string> Images { get; set; }
        public string Manufacturer { get; set; }
        public Guid ManufacturerId { get; set; }
        public string Brand { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public Guid? BrandId { get; set; }

    }
}
