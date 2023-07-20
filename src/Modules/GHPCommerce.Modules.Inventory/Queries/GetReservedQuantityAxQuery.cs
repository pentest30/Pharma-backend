using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Inventory.Entities;
using GHPCommerce.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Queries
{
    public class GetReservedQuantityAxQuery : ICommand<double>
    {
        public string InternalBatchNumber { get; set; }
        public string ProductCode { get; set; }
    }

    public class GetReservedQuantityAxQueryHandler : ICommandHandler<GetReservedQuantityAxQuery, double>
    {
        private readonly ICache _redisCache;
        private readonly ICurrentOrganization _currentOrganization;

        private readonly ConnectionStrings _connectionStrings;
        private readonly IRepository<InventSum, Guid> _inventSumRepository;

        public GetReservedQuantityAxQueryHandler(ICache redisCache,
            ICurrentOrganization currentOrganization, ConnectionStrings connectionStrings,
            IRepository<InventSum, Guid> inventSumRepository)

        {
            _redisCache = redisCache;
            _currentOrganization = currentOrganization;
            _connectionStrings = connectionStrings;
            _inventSumRepository = inventSumRepository;
        }

        public async Task<double> Handle(GetReservedQuantityAxQuery request, CancellationToken cancellationToken)
        {
            try
            {
                //await LockProvider<string>.WaitAsync(request.ProductCode, cancellationToken);
                var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (!org.HasValue)
                    return 0;
                var existingDim = await _inventSumRepository.Table
                    .AsTracking()
                    .FirstOrDefaultAsync(x => x.OrganizationId == org.Value
                                              && x.InternalBatchNumber == request.InternalBatchNumber
                                              && x.ProductCode == request.ProductCode, cancellationToken);
                if (existingDim == null)
                    return 0;
                string key = existingDim.ProductId.ToString() + org.Value;
                var inventSum = await _redisCache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);
                if (inventSum == null) return 0;
                var indexOfExistingDim = inventSum.CachedInventSumCollection.CachedInventSums.FindIndex(t =>
                    t.InternalBatchNumber == existingDim.InternalBatchNumber && t.ProductId == existingDim.ProductId &&
                    existingDim.OrganizationId == org.Value);
                if (indexOfExistingDim < 0) return 0;
                return inventSum.CachedInventSumCollection.CachedInventSums[indexOfExistingDim].PhysicalReservedQuantity;

            }

            catch (Exception e)
            {

                return 0;
            }
        }
    }

}
