using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.DTOs;
using GHPCommerce.Modules.Inventory.DTOs.Batches;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Queries.Transactions
{
    public class GetTransactionPagedQuery: ICommand<SyncPagedResult<InventItemTransactionDto>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }

    public class GetTransactionPagedQueryHandler : ICommandHandler<GetTransactionPagedQuery, SyncPagedResult<InventItemTransactionDto>>
    {
        private readonly IRepository<InventItemTransaction, Guid> _transactionRepository;
        private readonly IRepository<Entities.Invent, Guid> _inventRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public GetTransactionPagedQueryHandler(
            IRepository<InventItemTransaction, Guid> transactionRepository,
            IRepository<Entities.Invent, Guid> inventRepository,
            ICurrentOrganization currentOrganization,
            IMapper mapper,
            ICommandBus commandBus)
        {
            _transactionRepository = transactionRepository;
            _inventRepository = inventRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _commandBus = commandBus;
        }
        
        public async Task<SyncPagedResult<InventItemTransactionDto>> Handle(GetTransactionPagedQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new SyncPagedResult<InventItemTransactionDto>();
            var query = _transactionRepository.Table
                .Include(x=>x.Invent)
                .Where(x => x.OrganizationId == org)
                .DynamicWhereQuery(request.SyncDataGridQuery);
            if (request.SyncDataGridQuery.Where != null )
            {
                foreach (var wherePredicate in request.SyncDataGridQuery.Where[0].Predicates)
                {
                    if(wherePredicate.Value == null) 
                        continue;
                    if (wherePredicate.Field == "zoneId")
                        query = query.Where(x=>x.Invent.ZoneId == Guid.Parse(wherePredicate.Value.ToString()));
                        
                    if (wherePredicate.Field == "stockStateId")
                        query = query.Where(x=>x.Invent.StockStateId == Guid.Parse(wherePredicate.Value.ToString()));


                }
            
            }   
            var total = await query.CountAsync(cancellationToken: cancellationToken);
            var result = await query
              //  .OrderByDescending(x => x.CreatedDateTime)
                .Paged(request.SyncDataGridQuery.Skip / request.SyncDataGridQuery.Take + 1,
                    request.SyncDataGridQuery.Take)
                .ToListAsync(cancellationToken);
            var data = _mapper.Map<List<InventItemTransactionDto>>(result);
           
            return new SyncPagedResult<InventItemTransactionDto>
            {
                Result = data, Count = total
            };
        }   
        private string GetTransactionTypeStatus(int status)
        {
            return status switch
            {
                10 => "SupplierReception",
                20 => "SupplierInvoice",
                30 => "CustomerReturn",
                40 => "Readjustment",
                50 => "InterUnitTransfer",
                60 => "DeliveryNote",
                70 => "CustomerInvoice",
                80 => "Incineration",
                _ => String.Empty
            };
        }
    }
}