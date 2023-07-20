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
    public class DeleteInvoiceItemCommand : ICommand<ValidationResult>
    {
        public Guid InvoiceId { get; set; }
        public Guid ProductId { get; set; }
        public string InternalBatchNumber { get; set; }
    }
     public class DeleteInvoiceItemCommandHandler : ICommandHandler<DeleteInvoiceItemCommand, ValidationResult>
     {
         private readonly IRepository<SupplierInvoice, Guid> _repository;
         private readonly ICurrentOrganization _currentOrganization;

         public DeleteInvoiceItemCommandHandler(IRepository<SupplierInvoice, Guid> repository,
             ICurrentOrganization currentOrganization)
         {
             _repository = repository;
             _currentOrganization = currentOrganization;
         }
         public async Task<ValidationResult> Handle(DeleteInvoiceItemCommand request, CancellationToken cancellationToken)
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
                 var index = invoice.Items.FindIndex(x =>
                     x.ProductId == request.ProductId && x.InternalBatchNumber == request.InternalBatchNumber);
                 if(index<0)
                     throw new InvalidOperationException($"Ligne facture non trouvée");
                 invoice.Items.RemoveAt(index);
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