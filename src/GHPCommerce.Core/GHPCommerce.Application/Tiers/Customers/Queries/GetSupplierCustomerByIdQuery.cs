using System;
using GHPCommerce.Application.Tiers.Suppliers.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Customers.Queries
{
    public class GetSupplierCustomerByIdQuery :ICommand<SupplierCustomerDto>
    {
        public Guid Id { get; set; }
        public Guid CustomerOrganizationId { get; set; }
    }
}
