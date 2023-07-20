using System;
using GHPCommerce.Application.Tiers.Customers.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Customers.Queries
{
    public class GetCustomerForSalesPersonById : ICommand<CustomerDto>
    {
        public Guid CustomerId { get; set; }
    }
}
