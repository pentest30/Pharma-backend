using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.DTOs.Invoices;
using GHPCommerce.Modules.Sales.Entities.Billing;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Sales.Queries.Invoices
{
    public class GetInvoiceByCustomerIdQuery : ICommand<SyncPagedResult<InvoiceDto>>
    {
        public Guid CustomerId { get; set; }
        public SyncDataGridQuery SyncDataGridQuery { get; set; }

    }

    public class GetInvoiceByCustomerIdQueryHandler : ICommandHandler<GetInvoiceByCustomerIdQuery, SyncPagedResult<InvoiceDto>>
    {
        private readonly IRepository<Invoice, Guid> _invoiceRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public GetInvoiceByCustomerIdQueryHandler(IRepository<Invoice, Guid> invoiceRepository,
            ICurrentOrganization currentOrganization,
            IMapper mapper, ICommandBus commandBus,
            ICurrentUser currentUser)
        {
            _invoiceRepository = invoiceRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _commandBus = commandBus;
        }

        public async Task<SyncPagedResult<InvoiceDto>> Handle(GetInvoiceByCustomerIdQuery request,
            CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var query = _invoiceRepository.Table
                .Include(c => c.InvoiceItems)
                .OrderByDescending(x => x.CreatedDateTime)
                .Where(c => c.CustomerId == request.CustomerId)
                .DynamicWhereQuery(request.SyncDataGridQuery);

            var result = await query
                .OrderByDescending(x => x.CreatedDateTime)
                .Page(request.SyncDataGridQuery.Skip / request.SyncDataGridQuery.Take + 1,
                    request.SyncDataGridQuery.Take)
                .ToListAsync(cancellationToken);
            var invoices = _mapper.Map<List<InvoiceDto>>(result);
            foreach (var invoice in invoices)
            {
                invoice.InvoiceId = invoice.Id;
                var createUser =
                    await _commandBus.SendAsync(new GetUserQuery { Id = invoice.CreatedByUserId }, cancellationToken);
                if (createUser != null) invoice.CreatedBy = createUser.UserName;
                if (invoice.UpdatedByUserId != default)
                {
                    var updateUser = await _commandBus.SendAsync(new GetUserQuery { Id = invoice.UpdatedByUserId },
                        cancellationToken);
                    if (updateUser != null) invoice.UpdatedBy = updateUser.UserName;
                }
            }


            return new SyncPagedResult<InvoiceDto>
            {
                Result = invoices,
                Count = await query.CountAsync(cancellationToken)
            };
        }
    }
}