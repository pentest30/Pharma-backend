using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Tiers.Suppliers.Queries;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Core.Shared.Contracts.Inventory.Queries;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.DTOs.Invent;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Queries.Invent
{
   

    public class GetPagedInventByProductSyncQueryHandler : ICommandHandler<GetAvailableProductIdQuery,
            List<Guid>>
    {
        private readonly IMapper _mapper;
        private readonly IRepository<InventSum, Guid> _inventSumRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICache _redisCache;
        private readonly MedIJKModel _model;
        public GetPagedInventByProductSyncQueryHandler(IMapper mapper,
            IRepository<InventSum, Guid> inventSumRepository,
            ICurrentOrganization currentOrganization,
            ICommandBus commandBus,
            ICache redisCache, MedIJKModel model)
        {
            _mapper = mapper;
            _inventSumRepository = inventSumRepository;
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _redisCache = redisCache;
            _model = model;
        }
      

        public async Task<List<Guid>> Handle(GetAvailableProductIdQuery request, CancellationToken cancellationToken)
        { 
            if (Guid.Empty == request.OrganizationId)
                throw new InvalidOperationException("");
            var query = await _inventSumRepository.Table
                .Where(x => x.OrganizationId == request.OrganizationId && (x.PhysicalOnhandQuantity - x.PhysicalReservedQuantity) > 0 && x.IsPublic && x.ExpiryDate.HasValue && x.ExpiryDate > DateTime.Now)
                .GroupBy(c => c.ProductId)
                .Select(c => c.Key)
                .ToListAsync();
            return query;
            
        }
    }
}