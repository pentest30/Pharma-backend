using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Catalog.Products.Queries
{
    
    public class GetQuotaProductByIdQueryHandler : ICommandHandler<GetQuotaProductByIdQuery, bool>
    {
        private readonly IRepository<Product, Guid> _repository;

        public GetQuotaProductByIdQueryHandler(IRepository<Product, Guid> repository)
        {
            _repository = repository;
        }
        public Task<bool> Handle(GetQuotaProductByIdQuery request, CancellationToken cancellationToken)
        {
            return _repository.Table.AsNoTracking().Where(x => x.Id == request.ProductId).Select(x => x.Quota)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }
    }
}