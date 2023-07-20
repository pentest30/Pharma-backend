using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.DTOs.StockState;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Modules.Inventory.Queries.StockState
{
    public class GetAllStockStateQuery : ICommand<IEnumerable<StockStateDto>>
    {
    }
  
    public class GetAllStockStateQueryHandler : ICommandHandler<GetAllStockStateQuery, IEnumerable<StockStateDto>>
    {
        private readonly IRepository<Entities.StockState, Guid> _stockStateRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;


        public GetAllStockStateQueryHandler(
            IRepository<Entities.StockState, Guid> stockStateRepository,
           IMapper mapper,
           ICurrentOrganization currentOrganization)
        {
            _stockStateRepository = stockStateRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
        }

        public async Task<IEnumerable<StockStateDto>> Handle(GetAllStockStateQuery request, CancellationToken cancellationToken)
        {
            var query = await _stockStateRepository
                                  .Table
                                  .ToListAsync(cancellationToken);
            var result = _mapper.Map<List<StockStateDto>>(query);

            return result;
        }
    }
}
