using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.SupplierInvoices.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Procurement.Queries.Invoices
{
    public class GetInternalBnInvoiceQueryHandler : ICommandHandler<GetInternalBnInvoiceQuery, int>
    {
        private readonly IRepository<SupplierInvoice, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;

        public GetInternalBnInvoiceQueryHandler(IRepository<SupplierInvoice, Guid> repository, ICurrentOrganization currentOrganization)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
        }

        public async Task<int> Handle(GetInternalBnInvoiceQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var query = await (from q in _repository.Table
                    //.Include(x => x.Items)
                from item in q.Items.DefaultIfEmpty()
                where item!=null 
                      && q !=null
                      &&  q.CustomerId == orgId.Value
                      && item.ProductId == request.ProductId 
                      && q.SupplierId == request.SupplierId 
                      && item.VendorBatchNumber == request.VendorBatchNumber
                      orderby item.CreatedDateTime  
                select item).CountAsync(cancellationToken: cancellationToken);

            return  query;
        }
    }
}