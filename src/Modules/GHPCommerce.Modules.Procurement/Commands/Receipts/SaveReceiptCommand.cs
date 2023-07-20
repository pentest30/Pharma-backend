using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Procurement.Commands.Receipts
{
    public class SaveReceiptCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
    }
    public class  SaveReceiptCommandHandler : ICommandHandler<SaveReceiptCommand, ValidationResult>
    {
        private readonly IRepository<DeliveryReceipt, Guid> _repository;

        public SaveReceiptCommandHandler(IRepository<DeliveryReceipt, Guid> repository)
        {
            _repository = repository;
        }
        public async Task<ValidationResult> Handle(SaveReceiptCommand request, CancellationToken cancellationToken)
        {
            var invoice = await _repository
                .Table
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (invoice == null) throw new NotFoundException($"Receipt with id {request.Id} was not found");
            invoice.Status = InvoiceStatus.Saved;
            _repository.Update(invoice);
            await _repository.UnitOfWork.SaveChangesAsync();
            return default;
           
        }
    }
}