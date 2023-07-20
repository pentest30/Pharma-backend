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
    public class DeleteReceiptCommand: SaveReceiptCommand
    {
    }
     public  class  DeleteReceiptCommandHandler : ICommandHandler<DeleteReceiptCommand, ValidationResult>
     {
         private readonly IRepository<DeliveryReceipt, Guid> _repository;
         private readonly ICurrentOrganization _currentOrganization;

         public DeleteReceiptCommandHandler(IRepository<DeliveryReceipt, Guid> repository, ICurrentOrganization currentOrganization)
         {
             _repository = repository;
             _currentOrganization = currentOrganization;
         }
         public async Task<ValidationResult> Handle(DeleteReceiptCommand request, CancellationToken cancellationToken)
         {
             try
             {
                 var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                 if (orgId == null)
                     throw new InvalidOperationException($"resources not allowed");

                 var deliveryReceipt = await _repository
                     .Table
                     .Include(x=>x.Items)
                     .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
                 if (deliveryReceipt == null) throw new NotFoundException($"Receipt with id {request.Id} was not found");
                 deliveryReceipt.Status = InvoiceStatus.Removed;
                 _repository.Update(deliveryReceipt);
                 await _repository.UnitOfWork.SaveChangesAsync();
                 return default!;
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