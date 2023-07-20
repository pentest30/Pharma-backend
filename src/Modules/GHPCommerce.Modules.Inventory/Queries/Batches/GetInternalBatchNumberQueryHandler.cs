using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Batches.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Queries.Batches
{
    public class GetInternalBatchNumberQueryHandler : ICommandHandler<GetInternalBatchNumberQuery, object>
    {
        private readonly IRepository<Batch, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;

        public GetInternalBatchNumberQueryHandler(IRepository<Batch, Guid> repository, ICurrentOrganization currentOrganization)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
        }

        public async Task<object> Handle(GetInternalBatchNumberQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null) throw new InvalidOperationException("Resources not allowed");
            var query = await _repository.Table
                .Where(p => p.VendorBatchNumber == request.VendorBatchNumber
                                          && p.SupplierId == request.SupplierId
                                          && p.ProductId == request.ProductId
                                          && p.OrganizationId == orgId
                                          && p.ExpiryDate.Value.Date == request.ExpiryDate.Date
                                        )
                .Select(x=> new
                {
                    BatchNumber = x.InternalBatchNumber,
                    PPA = x.PpaHT,
                    PFS = x.PFS,
                    PurchaseUnitPrice = x.PurchaseUnitPrice,
                    Discount = x.PurchaseDiscountRatio,
                    SalePrice = x.SalesUnitPrice,
                    ExpiryDate = x.ExpiryDate,
                    Packing = x.packing
                })
                .ToListAsync(cancellationToken: cancellationToken);

            return query;
        }
    }
}
