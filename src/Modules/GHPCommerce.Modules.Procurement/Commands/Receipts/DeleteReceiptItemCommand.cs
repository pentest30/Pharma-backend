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

namespace GHPCommerce.Modules.Procurement.Commands.Receipts
{
    public class DeleteReceiptItemCommand  : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string InternalBatchNumber { get; set; }
        /// <summary>
        /// Montant total TTC
        /// </summary>
        public decimal TotalAmount { get; set; }
        /// <summary>
        /// Montant total TVA
        /// </summary>
        public decimal ReceiptsAmountExcTax { get; set; }
        /// <summary>
        /// Montant total des remises
        /// </summary>
    }
    public  class DeleteReceiptItemCommandHandler : ICommandHandler<DeleteReceiptItemCommand, ValidationResult>
     {
         private readonly IRepository<DeliveryReceipt, Guid> _repository;
         private readonly ICurrentOrganization _currentOrganization;

         public DeleteReceiptItemCommandHandler(IRepository<DeliveryReceipt, Guid> repository, ICurrentOrganization currentOrganization)
         {
             _repository = repository;
             _currentOrganization = currentOrganization;
         }
         public async Task<ValidationResult> Handle(DeleteReceiptItemCommand request, CancellationToken cancellationToken)
         {
             
             try
             {
                 var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                 if (orgId == null)
                     throw new InvalidOperationException($"resources not allowed");

                 var invoice = await _repository
                     .Table
                     .Include(x=>x.Items)
                     .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
                 if (invoice == null) throw new NotFoundException($"receipt with id {request.Id} was not found");
                 var index = invoice.Items.FindIndex(x =>
                     x.ProductId == request.ProductId && x.InternalBatchNumber == request.InternalBatchNumber);
                 if(index<0)
                     throw new InvalidOperationException($"Ligne Réception non trouvée");
                var item = invoice.Items[index];
                invoice.Items.RemoveAt(index);
                invoice.TotalAmount = request.TotalAmount;
                invoice.ReceiptsAmountExcTax = request.ReceiptsAmountExcTax;
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