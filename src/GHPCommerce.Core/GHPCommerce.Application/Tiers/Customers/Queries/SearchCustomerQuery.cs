using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Application.Tiers.Customers.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Tiers.Customers.Queries
{
    public class SearchCustomerQuery : ICommand<IEnumerable<CustomerDto>>
    {
        public string Term { get; set; }
    }
    public  class  SearchCustomerQueryHandler : ICommandHandler<SearchCustomerQuery, IEnumerable<CustomerDto>>
    {
        private readonly IRepository<Customer, Guid> _customerRepository;
        private readonly ICurrentOrganization _currentOrganization;

        public SearchCustomerQueryHandler(IRepository<Customer, Guid> customerRepository, ICurrentOrganization currentOrganization)
        {
            _customerRepository = customerRepository;
            _currentOrganization = currentOrganization;
        }

        public async Task<IEnumerable<CustomerDto>> Handle(SearchCustomerQuery request,CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var query = _customerRepository.Table
                .Include(x => x.Organization)
                .Include(x => x.SupplierCustomers)
                .Include("SupplierCustomers.Customer")
                .Include("SupplierCustomers.Customer.Organization")
                .Include("SupplierCustomers.Supplier")
                .SelectMany(x => x.SupplierCustomers)
                .Where(x => x.Supplier.OrganizationId == orgId && x.Customer.Organization.Name.Contains(request.Term));
            var result = await query
                .Select(x => new CustomerDto
                {
                    Id = (Guid)x.CustomerId,
                    CustomerGroup = x.CustomerGroup,
                    OrganizationId = x.Customer.OrganizationId,
                    Code = x.Code,
                    Name = x.Customer.Organization.Name,
                }).ToListAsync(cancellationToken);
            return result.Distinct().ToList();
        }
    }
}