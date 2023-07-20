using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Procurement.Commands.Invoices
{
    public class DeleteInvoiceCommand :SaveInvoiceCommand
    {
        
    }
    public  class  DeleteInvoiceCommandHandler : ICommandHandler<DeleteInvoiceCommand, ValidationResult>
    {
        private readonly IRepository<SupplierInvoice, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;

        public DeleteInvoiceCommandHandler(IRepository<SupplierInvoice, Guid> repository,
            ICurrentOrganization currentOrganization)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
        }
        public async Task<ValidationResult> Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (orgId == null)
                    throw new InvalidOperationException($"resources not allowed");

                var invoice = await _repository
                    .Table
                    .Include(x=>x.Items)
                    .FirstOrDefaultAsync(x => x.Id == request.InvoiceId, cancellationToken: cancellationToken);
                if (invoice == null) throw new NotFoundException($"invoice with id {request.InvoiceId} was not found");
                invoice.InvoiceStatus = InvoiceStatus.Removed;
                _repository.Update(invoice);
                await _repository.UnitOfWork.SaveChangesAsync();
                return default;
            }
            catch (Exception ex)
            {
                var  validations = new ValidationResult
                    {Errors = {new ValidationFailure("Transaction rolled back", ex.Message)}};
                return validations;
            }

        }
    }
}