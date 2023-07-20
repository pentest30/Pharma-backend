using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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
    public class GetInvoicesTurnOverByCustomerQuery : ICommand<decimal>
    {
        public Guid CustomerId { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }
    public class GetAllInvoicesForSalesPersonByCustomerQuery : ICommand<SyncPagedResult<InvoiceDtoV1>>
    {
        public DateTime? Start { get; set; }
        public DateTime?  End { get; set; }
        public Guid CustomerId { get; set; }
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }
    public class GetAllInvoicesForSalesPersonByCustomerQueryV1 : ICommand<List<InvoiceDtoV1>>
    {
        public Guid CustomerId { get; set; }
        public DateTime? Start { get; set; }
        public DateTime?  End { get; set; }
    }
     public class  GetAllInvoicesForSalesPersonByCustomerQueryHandler : ICommandHandler<GetAllInvoicesForSalesPersonByCustomerQuery, SyncPagedResult<InvoiceDtoV1>>,
         ICommandHandler<GetInvoicesTurnOverByCustomerQuery, decimal>,
         ICommandHandler< GetAllInvoicesForSalesPersonByCustomerQueryV1,List<InvoiceDtoV1>>
     {
         private readonly IRepository<Invoice, Guid> _invoiceRepository;
         private readonly ICurrentOrganization _currentOrganization;
         private readonly IMapper _mapper;
         private readonly ICommandBus _commandBus;

         public GetAllInvoicesForSalesPersonByCustomerQueryHandler(IRepository<Invoice, Guid> invoiceRepository,
             ICurrentOrganization currentOrganization,
             IMapper mapper, ICommandBus commandBus,
             ICurrentUser currentUser)
         {
             _invoiceRepository = invoiceRepository;
             _currentOrganization = currentOrganization;
             _mapper = mapper;
             _commandBus = commandBus;
         }

         public async Task<SyncPagedResult<InvoiceDtoV1>> Handle(GetAllInvoicesForSalesPersonByCustomerQuery request, CancellationToken cancellationToken)
         {
             // var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
             var query = _invoiceRepository.Table
                 .Include(c => c.InvoiceItems)
                 .Where(x => x.CustomerId == request.CustomerId)
                 .DynamicWhereQuery(request.SyncDataGridQuery);
             if (request.Start.HasValue)
                 query = query.Where(x => x.InvoiceDate.Date >= request.Start.Value.Date);
             if (request.End.HasValue)
                 query = query.Where(x => x.InvoiceDate.Date <= request.End.Value.Date);

             var result = await query
                 .Page(request.SyncDataGridQuery.Skip / request.SyncDataGridQuery.Take + 1,
                     request.SyncDataGridQuery.Take)
                 .ToListAsync(cancellationToken);
             return new CustomSyncPagedResult<InvoiceDtoV1>
             {
                 Result = _mapper.Map<List<InvoiceDtoV1>>(result),
                 Count = await query.CountAsync(cancellationToken),

             };
         }

         public async Task<decimal> Handle(GetInvoicesTurnOverByCustomerQuery request, CancellationToken cancellationToken)
         {
             var turnover = _invoiceRepository.Table
                 .Where(x => x.CustomerId == request.CustomerId);
             if (request.Start.HasValue)
                 turnover = turnover.Where(x => x.InvoiceDate >= request.Start.Value.Date);
             if (request.End.HasValue)
                 turnover = turnover.Where(x => x.InvoiceDate <= request.End.Value.Date);
             return await turnover.SumAsync(x => x.TotalTTC, cancellationToken);

         }

         public async Task<List<InvoiceDtoV1>> Handle(GetAllInvoicesForSalesPersonByCustomerQueryV1 request, CancellationToken cancellationToken)
         {
             var query = _invoiceRepository.Table
                 .Include(c => c.InvoiceItems)
                 .Where(x => x.CustomerId == request.CustomerId);
             if (request.Start.HasValue)
                 query = query.Where(x => x.InvoiceDate.Date >= request.Start.Value.Date);
             if (request.End.HasValue)
                 query = query.Where(x => x.InvoiceDate.Date <= request.End.Value.Date);
                 
                 //.ToListAsync(cancellationToken: cancellationToken);
             return _mapper.Map<List<InvoiceDtoV1>>(await query.ToListAsync(cancellationToken: cancellationToken));

         }
     }
}