using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Procurement.DTOs;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Procurement.Queries.DeliveryReceipts
{
    public class GetPagedDeliveryReceiptsQuery : ICommand<SyncPagedResult<DeliveryReceiptDto>>
    {
        public SyncDataGridQuery Query { get; set; }
    }
    public class GetPagedDeliveryReceiptsQueryHandler : ICommandHandler<GetPagedDeliveryReceiptsQuery, SyncPagedResult<DeliveryReceiptDto>>
    {
        private readonly IRepository<DeliveryReceipt, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public GetPagedDeliveryReceiptsQueryHandler(IRepository<DeliveryReceipt, Guid> repository,  
            ICurrentOrganization currentOrganization, ICommandBus commandBus,
            IMapper mapper)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _commandBus = commandBus;

        }
        public async Task<SyncPagedResult<DeliveryReceiptDto>> Handle(GetPagedDeliveryReceiptsQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var query = _repository.Table
                .Include(c => c.Items)
                .OrderByDescending(x => x.CreatedDateTime)
                .Where(x => x.OrganizationId == orgId
                            && x.Status != InvoiceStatus.Removed)
                .DynamicWhereQuery(request.Query);

            var result = await query
               // .OrderByDescending(x => x.CreatedDateTime)
                .Page(request.Query.Skip / request.Query.Take + 1,
                    request.Query.Take)
                .ToListAsync(cancellationToken);
            var data = _mapper.Map<List<DeliveryReceiptDto>>(result);
            foreach (var deliveryReceipt in data)
            {
                deliveryReceipt.DeliveryReceiptStatus = GetDeliveryReceiptStatusStatus((int)deliveryReceipt.Status);
                deliveryReceipt.DeliveryReceiptId = deliveryReceipt.Id;
                var createUser = await _commandBus.SendAsync(new GetUserQuery { Id = deliveryReceipt.CreatedByUserId }, cancellationToken);
                if (createUser != null) deliveryReceipt.CreatedBy = createUser.UserName;
                if (deliveryReceipt.UpdatedByUserId != default)
                {
                    var updateUser = await _commandBus.SendAsync(new GetUserQuery { Id = deliveryReceipt.UpdatedByUserId }, cancellationToken);
                    if (updateUser != null) deliveryReceipt.UpdatedBy = updateUser.UserName;
                }
            }
            return new SyncPagedResult<DeliveryReceiptDto>
            {
                Result = data,
                Count = await query.CountAsync(cancellationToken)
            };
        }
        private string GetDeliveryReceiptStatusStatus(int status)
        {
            return status switch
            {
                0 => "Créé",
                1 => "Enregistré",
                2 => "En cours de traitement",
                3 => "Cloturé",
                4 => "Validé",

                _ => String.Empty
            };
        }
    }
}