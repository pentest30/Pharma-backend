using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.DTOs.StockState;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Modules.Inventory.Queries.StockState
{
    public class GetStockStateByIdQuery : ICommand<StockStateDto>
    {
        public Guid Id { get; set; }
    }
    public class GetStockStateByIdQueryHandler : ICommandHandler<GetStockStateByIdQuery, StockStateDto>
    {
        private readonly IRepository<Entities.StockState, Guid> _stockStateRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;


        public GetStockStateByIdQueryHandler(
            IRepository<Entities.StockState, Guid> stockStateRepository,
           IMapper mapper,
           ICurrentOrganization currentOrganization)
        {
            _stockStateRepository = stockStateRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
        }

       
        async Task<StockStateDto> IRequestHandler<GetStockStateByIdQuery, StockStateDto>.Handle(GetStockStateByIdQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return default;
            var entity = await _stockStateRepository.Table
                .FirstOrDefaultAsync(x => x.Id == request.Id
                                          && x.OrganizationId == org, cancellationToken);

            return _mapper.Map<StockStateDto>(entity);
        }
    }
}
