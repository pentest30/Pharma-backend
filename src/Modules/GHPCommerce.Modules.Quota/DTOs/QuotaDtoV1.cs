using System;

namespace GHPCommerce.Modules.Quota.DTOs
{
    public class QuotaDtoV1
    {
        public Guid Id { get; set; }
        public int AvailableQuantity { get; set; }
    }
}
