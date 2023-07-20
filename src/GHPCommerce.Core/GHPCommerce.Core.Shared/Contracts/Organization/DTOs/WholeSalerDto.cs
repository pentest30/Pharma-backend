using System;

namespace GHPCommerce.Core.Shared.Contracts.Organization.DTOs
{
    public class WholeSalerDto
    {
        public string Name { get; set; }
        public Guid? DefaultSalesPersonId { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }

    }
}
