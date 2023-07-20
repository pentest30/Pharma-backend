using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Transactions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.Entities;

namespace GHPCommerce.Modules.Inventory.Commands.Transactions
{
    public class CreateAtSupplierInventTransactionCommandHandler : ICommandHandler<CreateAtSupplierInventTransactionCommand, ValidationResult>
    {
        private readonly IRepository<InventItemTransaction, Guid> _transactionRepository;
        private readonly ICurrentUser _currentUser;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;

        public CreateAtSupplierInventTransactionCommandHandler(IRepository<InventItemTransaction , Guid> transactionRepository, 
            ICurrentUser currentUser, 
            ICurrentOrganization currentOrganization, 
            IMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _currentUser = currentUser;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
        }
        public async Task<ValidationResult> Handle(CreateAtSupplierInventTransactionCommand request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new ValidationResult { Errors = { new ValidationFailure("User Not valid ", "User not assigned to any organization") } };
            var validator = new CreateAtSupplierInventTransactionCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
                return validationErrors;
            var transaction = _mapper.Map<InventItemTransaction>(request);
            transaction.OrganizationId = org.Value;
            transaction.CustomerId = request.CustomerId;
            transaction.TransactionType = TransactionType.SupplierInvoice;
            transaction.StockEntry = true;
            _transactionRepository.Add(transaction);
            await _transactionRepository.UnitOfWork.SaveChangesAsync();
            return default;
        }
    }
    
}