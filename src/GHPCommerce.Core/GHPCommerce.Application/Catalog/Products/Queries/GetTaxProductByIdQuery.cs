using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Catalog.Products.Queries
{
    public class GetTaxProductByIdQuery : ICommand<decimal>
    {
        public Guid ProductId { get; set; }
    }
    public class GetTaxProductByIdQueryHandler : ICommandHandler<GetTaxProductByIdQuery, decimal>
    {
        private readonly IRepository<Product, Guid> _repository;

        public GetTaxProductByIdQueryHandler(IRepository<Product, Guid> repository)
        {
            _repository = repository;
        }
        public Task<decimal> Handle(GetTaxProductByIdQuery request, CancellationToken cancellationToken)
        {
            return _repository.Table.AsNoTracking().Include(c => c.TaxGroup).Where(x => x.Id == request.ProductId).Select(x => x.TaxGroup.TaxValue)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }
    }
}