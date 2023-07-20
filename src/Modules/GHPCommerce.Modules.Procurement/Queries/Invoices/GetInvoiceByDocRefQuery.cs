using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Procurement.DTOs;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Procurement.Queries.Invoices
{
    public class GetInvoiceByDocRefQuery : ICommand<SupplierInvoiceDto>
    {
        public DateTime InvoiceDate { get; set; }
        public string DocumentRef { get; set; }
        public Guid SupplierId { get; set; }

    }
    public class GetInvoiceByDocRefQueryHandler : ICommandHandler<GetInvoiceByDocRefQuery, SupplierInvoiceDto>
    {
        private readonly IRepository<SupplierInvoice, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;

        public GetInvoiceByDocRefQueryHandler(IRepository<SupplierInvoice, Guid> repository,
            ICurrentOrganization currentOrganization, IMapper mapper)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;

        }
        public async Task<SupplierInvoiceDto> Handle(GetInvoiceByDocRefQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!orgId.HasValue) throw new InvalidOperationException("");
            var invoice = await _repository.Table
                .AsNoTracking()
                .Where(x => x.RefDocument == request.DocumentRef
                            && x.InvoiceDate.Date == request.InvoiceDate.Date
                   && x.SupplierId == request.SupplierId
                   && x.CustomerId == orgId.Value)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            return _mapper.Map<SupplierInvoiceDto>(invoice);
        }
    }
}