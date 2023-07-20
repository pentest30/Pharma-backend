using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.DeliveryOrder.Dtos;
using GHPCommerce.Core.Shared.Contracts.DeliveryOrder.Queries;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.PreparationOrder.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.PreparationOrder.Queries.DeleiveryOrders
{
    public class GetDeliveryOrderByIdQueryHandler : ICommandHandler<GetDeliveryOrderByIdQuery, DeliveryOrderDtoV2>
    {
        private readonly IRepository<DeleiveryOrder, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;

        public GetDeliveryOrderByIdQueryHandler(IRepository<DeleiveryOrder, Guid> repository,
            ICurrentOrganization currentOrganization,
            IMapper mapper)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
        }

        public async Task<DeliveryOrderDtoV2> Handle(GetDeliveryOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var deliveryOrder =  await _repository
                .Table
                .AsNoTracking()
                .Include(x => x.DeleiveryOrderItems)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (deliveryOrder == null)
                throw new NotFoundException($"Delivery order with id: {request.Id} was not found");
            var result = _mapper.Map<DeliveryOrderDtoV2>(deliveryOrder);          
            return result;
        }
    }
}