using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Application.Catalog.TransactionTypes.Commands
{
    public class TransactionTypeCommandHandler :
        ICommandHandler<CreateTransactionTypeCommand, ValidationResult>,
        ICommandHandler<UpdateTransactionTypeCommand, ValidationResult>,
        ICommandHandler<DeleteTransactionTypeCommand>

    {
        private readonly IRepository<TransactionType, Guid> _transactionTypeRepository;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;


        public TransactionTypeCommandHandler(IRepository<TransactionType, Guid> transactionTypeRepository, IMapper mapper)
        {
            _transactionTypeRepository = transactionTypeRepository;
            _unitOfWork = _transactionTypeRepository.UnitOfWork;
            _mapper = mapper;

        }

        public async Task<ValidationResult> Handle(CreateTransactionTypeCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateTransactionTypeCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
                return validationErrors;
            var transactionType = _mapper.Map<TransactionType>(request);
            _transactionTypeRepository.Add(transactionType);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        async Task<ValidationResult> IRequestHandler<UpdateTransactionTypeCommand, ValidationResult>.Handle(UpdateTransactionTypeCommand request, CancellationToken cancellationToken)
        {
            var transactionType = await _transactionTypeRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            var validator = new UpdateTransactionTypeCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
                return validationErrors;
            transactionType.Blocked = request.Blocked;
            transactionType.TransactionTypeName = request.TransactionTypeName;
            transactionType.CodeTransaction = request.CodeTransaction;
            _transactionTypeRepository.Update(transactionType);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }
        public async Task<Unit> Handle(DeleteTransactionTypeCommand request, CancellationToken cancellationToken)
        {
            var zoneGroup =
              await _transactionTypeRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id,
                  cancellationToken: cancellationToken);
            if (zoneGroup == null)
                throw new NotFoundException($"Zone group with id: {request.Id} was not found");
          
            _transactionTypeRepository.Delete(zoneGroup);
            await _unitOfWork.SaveChangesAsync();
            return Unit.Value;
        }
    }
}
