using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.PreparationOrder.DTOs;
using GHPCommerce.Modules.PreparationOrder.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Orders.Queries;
using GHPCommerce.Core.Shared.Contracts.PreparationOrders.DTOs;
using GHPCommerce.Core.Shared.Contracts.PreparationOrders.Queries;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.CrossCuttingConcerns.Exceptions;

namespace GHPCommerce.Modules.PreparationOrder.Queries.Consolidation
{
    class ConsolidationQueriesHandler :
        ICommandHandler<GetPagedConsolidationOrdersQuery, SyncPagedResult<ConsolidationOrdersDto>>,
        ICommandHandler<GetConsolidationOrderByIdQuery, ConsolidationOrder>,
        ICommandHandler<GetConsolidationOrderByOrderIdQuery, ConsolidationOrder>,
        ICommandHandler<GetConsolidationOrderQuery,ConsolidationValidationDto>, 
        ICommandHandler<GetConsolidateOrderByIdQuery, bool>
    {
        private readonly IRepository<ConsolidationOrder, Guid> _repository;
        private readonly IRepository<DeleiveryOrder, Guid> _deliverOrderRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICurrentUser _currentUser;
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public ConsolidationQueriesHandler(
          IRepository<ConsolidationOrder, Guid> repository,
          IRepository<DeleiveryOrder, Guid> deliverOrderRepository,
          ICurrentOrganization currentOrganization,
          ICurrentUser currentUser,
          ICommandBus commandBus,
          IMapper mapper)
        {
            _repository = repository;
            _deliverOrderRepository = deliverOrderRepository;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _commandBus = commandBus;
            _mapper = mapper;
        }
        public async Task<SyncPagedResult<ConsolidationOrdersDto>> Handle(GetPagedConsolidationOrdersQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new SyncPagedResult<ConsolidationOrdersDto>();
  
            var query = _repository.Table
                .Where(x => x.OrganizationId == org)
                .DynamicWhereQuery(request.DataGridQuery);

            if (!String.IsNullOrEmpty(request.barCode))
            {
                var orderIdentifier = request.barCode.Substring(2, 9).TrimStart('0');
                query = query.Where(c => c.OrderIdentifier == orderIdentifier);
            }

            var total = await query.CountAsync(cancellationToken: cancellationToken);
           
            query = query
                 .Paged(request.DataGridQuery.Skip / request.DataGridQuery.Take + 1, request.DataGridQuery.Take);
            var consolidationOrdersList = _mapper.Map<List<ConsolidationOrdersDto>>(await query.ToListAsync(cancellationToken));
            foreach (var item in consolidationOrdersList)
            {
                var deliverOrder = await _deliverOrderRepository.Table.Where(c => c.OrderId == item.OrderId).FirstOrDefaultAsync();
                item.BlGenerated = (deliverOrder != null) ? true : false;
            }
            return new SyncPagedResult<ConsolidationOrdersDto>
            {
                Count = total,
                Result = consolidationOrdersList
            };
        }

        public async Task<ConsolidationOrder> Handle(GetConsolidationOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var preparationOrder = await _repository
                .Table
               
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (preparationOrder == null)
                throw new NotFoundException($"Consolidation Order with id: {request.Id} was not found");

            return preparationOrder;
        }
        public async Task<ConsolidationOrder> Handle(GetConsolidationOrderByOrderIdQuery request, CancellationToken cancellationToken)
        {
            var preparationOrder = await _repository
                .Table
                .FirstOrDefaultAsync(x => x.OrderId == request.OrderId, cancellationToken: cancellationToken);
            if (preparationOrder == null)
                throw new NotFoundException($"Consolidation Order with orderId: {request.OrderId} was not found");

            return preparationOrder;
        }
      
        public async Task<ConsolidationValidationDto> Handle(GetConsolidationOrderQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var errors = new Dictionary<string, string>();

            if (!org.HasValue)
            {
                errors.Add("Error", "Organisation est null");
                return new ConsolidationValidationDto { ErrorMessages = errors, ConsolidationOrder = default };
            }

            var query = _repository.Table
                .Where(x => x.OrganizationId == org);

            if (!String.IsNullOrEmpty(request.BarCode))
            {
                query = query.Where(c => c.OrderIdentifier == request.BarCode);
            }
            var result =await query.FirstOrDefaultAsync(cancellationToken);
           


            if (result != null)
            {
                // check if the order is not cancelled
                var order = await _commandBus.SendAsync(new GetOrderByIdQueryV2 { Id = result.OrderId }, cancellationToken);
                if (order.OrderStatus == 70)
                {
                    errors.Add("Error", "Ordre de  préparation Annulé");
                    return new ConsolidationValidationDto { ErrorMessages = errors, ConsolidationOrder = default };
                }
                var customer = await _commandBus.SendAsync(new GetCustomerByOrganizationIdQuery { OrganizationId = order.CustomerId }, cancellationToken);
                
                var data =  _mapper.Map<ConsolidationOrdersDto>(result);
                data.OrderStatus = order.OrderStatus;
                if (result.ReceivedInShippingId != null)
                    data.ReceivedInShippingById = result.ReceivedInShippingId.Value;
                data.Sector= customer?.Sector;
                return new ConsolidationValidationDto { ErrorMessages = errors, ConsolidationOrder = data };
            }

            errors.Add("Error", "Ordre de  préparation non trouvé");
            return new ConsolidationValidationDto { ErrorMessages = errors, ConsolidationOrder = default };
        }

        public async Task<bool> Handle(GetConsolidateOrderByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.Table.FirstOrDefaultAsync(x => x.OrderId == request.OrderId, cancellationToken: cancellationToken) != null;
        }
    }
    public class ConsolidationValidationDto
    {
        public Dictionary<string,string> ErrorMessages { get; set; }
        public ConsolidationOrdersDto ConsolidationOrder { get; set; }
    }
}
