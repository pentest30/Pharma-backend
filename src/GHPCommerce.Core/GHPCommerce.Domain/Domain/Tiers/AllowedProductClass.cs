using System;
using GHPCommerce.Domain.Domain.Catalog;

namespace GHPCommerce.Domain.Domain.Tiers
{
    public class AllowedProductClass
    {
        public Guid ProductClassId { get; set; }
        public Guid SupplierCustomerId { get; set; }
        public SupplierCustomer SupplierCustomer { get; set; }
        public ProductClass ProductClass  { get; set; }
    }
}
