using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Procurement.DTOs;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;

namespace GHPCommerce.Modules.Procurement.Queries.Invoices
{
    public class GetPagedSupplierInvoicesQuery : ICommand<SyncPagedResult<SupplierInvoiceDto>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
        public bool? ValidInvoice { get; set; }
    }
    public class GetPagedSupplierInvoicesQueryHandler : ICommandHandler<GetPagedSupplierInvoicesQuery, SyncPagedResult<SupplierInvoiceDto>>
    {
        private readonly IRepository<SupplierInvoice, Guid> _supplierInvoiceRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public GetPagedSupplierInvoicesQueryHandler(IRepository<SupplierInvoice, Guid> supplierInvoiceRepository,
            ICurrentOrganization currentOrganization,
            IMapper mapper, ICommandBus commandBus,
            ICurrentUser currentUser)
        {
            _supplierInvoiceRepository = supplierInvoiceRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _commandBus = commandBus;
        }

        public async Task<SyncPagedResult<SupplierInvoiceDto>> Handle(GetPagedSupplierInvoicesQuery request,
            CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var query = _supplierInvoiceRepository.Table
                .Include(c => c.Items)
                .OrderByDescending(x => x.CreatedDateTime)
                .Where(x => x.CustomerId == orgId
                            && x.InvoiceStatus !=InvoiceStatus.Removed)
                .DynamicWhereQuery(request.SyncDataGridQuery);

            if (request.ValidInvoice == true)
                query = query.Where(x => x.InvoiceStatus == InvoiceStatus.InProgress);
           
            var result = await query
              //  .OrderByDescending(x => x.CreatedDateTime)
                .Page(request.SyncDataGridQuery.Skip / request.SyncDataGridQuery.Take + 1,
                    request.SyncDataGridQuery.Take)
                .ToListAsync(cancellationToken);
         
            var dataForSalesPerson = _mapper.Map<List<SupplierInvoiceDto>>(result);
            foreach (var invoice in dataForSalesPerson)
            {
                invoice.InvoiceId = invoice.Id;
                invoice.Status = GetInvoiceStatus((int)invoice.InvoiceStatus);
                var createUser = await _commandBus.SendAsync(new GetUserQuery { Id = invoice.CreatedByUserId }, cancellationToken);
                if (createUser != null) invoice.CreatedBy = createUser.UserName;
                if (invoice.UpdatedByUserId != default)
                {
                    var updateUser = await _commandBus.SendAsync(new GetUserQuery { Id = invoice.UpdatedByUserId }, cancellationToken);
                    if (updateUser != null) invoice.UpdatedBy = updateUser.UserName;
                }
            }


            return new SyncPagedResult<SupplierInvoiceDto>
            {
                Result = dataForSalesPerson,
                Count = await query.CountAsync(cancellationToken)
            };
        }
        private string GetInvoiceStatus(int status)
        {
            return status switch
            {
                0 => "En attente",
                1 => "Enregistrée",
                2 => "En cours de traitement",
                3 => "Cloturée",
                4 => "Validée",

                _ => String.Empty
            };
        }


    }
}