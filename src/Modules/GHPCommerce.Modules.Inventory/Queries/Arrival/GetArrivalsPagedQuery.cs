using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Modules.Inventory.Entities;
using GHPCommerce.Modules.Inventory.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Queries.Arrival
{
    public class GetArrivalsPagedQuery: ICommand<SyncPagedResult<Arrivals>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }
    public class GetArrivalsPagedQueryCommandHandler : ICommandHandler<GetArrivalsPagedQuery, SyncPagedResult<Arrivals>>
    {
        private readonly Func<AxDbContext> _factory;

        public GetArrivalsPagedQueryCommandHandler(Func<AxDbContext> factory = null)
        {
            _factory = factory;
        }
        public  async Task<SyncPagedResult<Arrivals>> Handle(GetArrivalsPagedQuery request, CancellationToken cancellationToken)
        {
            await using var context = _factory.Invoke();
            var query = context.Set<Arrivals>()  
                .OrderBy(x=>x.ProductName)
                .AsQueryable();
            if (request.SyncDataGridQuery.Where != null)
            {
                foreach (var wherePredicate in request.SyncDataGridQuery.Where[0].Predicates)
                {
                    if (wherePredicate.Field == "productName")
                        query = query.Where(x =>x.ProductName.Contains(wherePredicate.Value.ToString()));
                    else if (wherePredicate.Field == "productCode")
                        query = query.Where(x => x.ProductCode.Contains(wherePredicate.Value.ToString()));
                    else if (wherePredicate.Field == "internalBatchNumber")
                        query = query.Where(x => x.InternalBatchNumber.Contains(wherePredicate.Value.ToString()));
                       
                }
            }

            var total = await query.CountAsync(cancellationToken);
            var result = await query
                 
                .Paged(request.SyncDataGridQuery.Skip / request.SyncDataGridQuery.Take + 1,
                    request.SyncDataGridQuery.Take)
                .ToListAsync(cancellationToken);
            return new SyncPagedResult<Arrivals> { Count = total, Result = result };
        }
    }
}