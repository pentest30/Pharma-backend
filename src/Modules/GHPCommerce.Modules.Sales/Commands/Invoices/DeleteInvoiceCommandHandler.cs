using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Invoices.Commands;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Sales.Entities.Billing;
using GHPCommerce.Modules.Sales.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;

namespace GHPCommerce.Modules.Sales.Commands.Invoices
{
    public class DeleteInvoiceCommandHandler : ICommandHandler<DeleteInvoiceCommand, ValidationResult>
    {

        private readonly ICommandBus _commandBus;
        private readonly IRepository<Invoice, Guid> _repository;
        private  SalesDbContext _context;

        public DeleteInvoiceCommandHandler(ICommandBus commandBus, IRepository<Invoice, Guid> repository, SalesDbContext context)
        {
            _commandBus = commandBus;
            _repository = repository;
            _context = context;

        }

        public async Task<ValidationResult> Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var invoice =
                    await _repository.Table.FirstOrDefaultAsync(c => c.OrderId == request.OrderId, cancellationToken);
                _repository.Delete(invoice);
                _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }

            return default;
        }
    }
}