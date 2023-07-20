using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Procurement.Commands.Invoices
{
    public class SaveInvoiceCommand :ICommand<ValidationResult>
    {
        public Guid InvoiceId { get; set; }
    }
     public class SaveInvoiceCommandHandler:ICommandHandler<SaveInvoiceCommand, ValidationResult>
     {
         private readonly IRepository<SupplierInvoice, Guid> _repository;

         public SaveInvoiceCommandHandler(IRepository<SupplierInvoice, Guid> repository)
         {
             _repository = repository;
         }
         public async Task<ValidationResult> Handle(SaveInvoiceCommand request, CancellationToken cancellationToken)
         {
             var invoice = await _repository
                 .Table
                 .FirstOrDefaultAsync(x => x.Id == request.InvoiceId, cancellationToken: cancellationToken);
             if (invoice == null) throw new NotFoundException($"invoice with id {request.InvoiceId} was not found");
             invoice.InvoiceStatus = InvoiceStatus.Saved;
             _repository.Update(invoice);
             await _repository.UnitOfWork.SaveChangesAsync();
             return default;

         }
     }
}