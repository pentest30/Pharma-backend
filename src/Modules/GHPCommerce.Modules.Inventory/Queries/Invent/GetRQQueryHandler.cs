using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Inventory.Dtos;
using GHPCommerce.Core.Shared.Contracts.Inventory.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using Microsoft.EntityFrameworkCore;


namespace GHPCommerce.Modules.Inventory.Queries.Invent
{
    public class GetRQQueryHandler : ICommandHandler<GetRQQuery, IEnumerable<InventRQDto>>
    {
        private readonly IRepository<Entities.Invent, Guid> _inventSumRepo;
        private readonly ICurrentOrganization _currentOrganization;

        public GetRQQueryHandler(IRepository<Entities.Invent, Guid> inventSumRepo, ICurrentOrganization currentOrganization)
        {
            _inventSumRepo = inventSumRepo;
            _currentOrganization = currentOrganization;
        }

        public async Task<IEnumerable<InventRQDto>> Handle(GetRQQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var zoneAtSuppId = Guid.Parse("7BD42E23-E657-4F99-AFEF-1AFE5CEACB16");
            var stateId = Guid.Parse("7BD52E22-E657-4F99-AFEF-1AFE5CEACB16");
            var ids = request.ProductIds.ToArray();
            var result = new List<InventRQDto>();
            var query = await (from invent in _inventSumRepo.Table
                    where invent.OrganizationId == org.Value
                        && ids.Any(p => p == invent.ProductId)
                        && invent.PhysicalQuantity > 0

                    select new InventDto1 
                    {
                        PhysicalQuantity = invent.PhysicalQuantity,
                        ProductId =invent.ProductId,
                        ZoneId= invent.ZoneId,
                        StockStateId= invent.StockStateId

                    })
                .ToListAsync(cancellationToken: cancellationToken);
            var atSuppQuery = query
                .Where(i => i.ZoneId == zoneAtSuppId)
                .ToList();
            var nonSalableQuery = query
                .Where(x=>x.StockStateId == stateId)
                .Except(atSuppQuery);
            foreach (var grouping in nonSalableQuery.GroupBy(x => x.ProductId))
            {
                double rq = 0;
                var item = atSuppQuery
                    .GroupBy(x=>x.ProductId)
                    .FirstOrDefault(x => x.Key == grouping.Key)?
                    .Sum(x => x.PhysicalQuantity);
                if (item.HasValue) rq = item.Value;
                var dto =new InventRQDto();
                dto.ProductId = grouping.Key;
                dto.QuotaQnt = Convert.ToDouble( grouping.Sum(x => x.PhysicalQuantity));
                dto.RemainQnt = rq;

                result.Add(dto);
            }
            foreach (var item in atSuppQuery.GroupBy(x=>x.ProductId))   
            {
                if (result.All(p => p.ProductId != item.Key))
                {
                    result.Add(new InventRQDto
                    {
                        ProductId = item.Key, RemainQnt = item.Sum(x => x.PhysicalQuantity), QuotaQnt = 0
                    });
                }
            }

            return result;
        }
    }

    public class InventDto1
    {
        public double PhysicalQuantity { get; set; }
        public Guid ProductId { get; set; }
        public Guid ZoneId { get; set; }
        public Guid StockStateId { get; set; }
    }
}