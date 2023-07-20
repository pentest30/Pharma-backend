using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Procurement.DTOs;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Procurement.Queries.Invoices
{
    public class GetInvoiceByIdQuery : ICommand<SupplierInvoiceDto>
    {
        public Guid InvoiceId { get; set; }
    }
     public  class  GetInvoiceByIdQueryHandler : ICommandHandler<GetInvoiceByIdQuery, SupplierInvoiceDto>
     {
         private readonly IRepository<SupplierInvoice, Guid> _repository;
         private readonly ICurrentOrganization _currentOrganization;
         private readonly IMapper _mapper;

         public GetInvoiceByIdQueryHandler(IRepository<SupplierInvoice, Guid> repository, 
             ICurrentOrganization currentOrganization,
             IMapper mapper)
         {
             _repository = repository;
             _currentOrganization = currentOrganization;
             _mapper = mapper;
         }
         public async Task<SupplierInvoiceDto> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
         {
             var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
             var query = await _repository.Table.Include(c => c.Items)
                 .OrderByDescending(x => x.CreatedDateTime)
                 .FirstOrDefaultAsync(x => x.CustomerId == orgId && request.InvoiceId == x.Id, cancellationToken: cancellationToken);
             return _mapper.Map<SupplierInvoiceDto>(query);
             
         }
     }
}