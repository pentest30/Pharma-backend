using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Catalog.INNs.Commands
{
   public class InnCommandsHandler : ICommandHandler<CreateInnCommand, ValidationResult>,
       ICommandHandler<UpdateInnCommand, ValidationResult>, 
       ICommandHandler<DeleteInnCommand>
   {
        private readonly IRepository<INN, Guid> _innRepository;
        private readonly IUnitOfWork _unitOfWork;
        public InnCommandsHandler(IRepository<INN, Guid> innRepository)
        {
            _innRepository = innRepository;
            _unitOfWork = _innRepository.UnitOfWork;
        }
        public async Task<ValidationResult> Handle(CreateInnCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateInnCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateName(request.Id, request.Name, cancellationToken, validationErrors);
            if (!validationErrors.IsValid)
                return validationErrors;
            var inn = new INN(request.Id, request.Name , request.Description);
            _innRepository.Add(inn);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<ValidationResult> Handle(UpdateInnCommand request, CancellationToken cancellationToken)
        {
            var inn = await _innRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id,
                cancellationToken: cancellationToken);
            if (inn == null)
                throw new NotFoundException($"INN with id: {request.Id} was not found");
            var validator = new UpdateInnCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateName(request.Id, request.Name, cancellationToken, validationErrors);
            if (!validationErrors.IsValid)
                return validationErrors;
            inn.Name = request.Name;
            inn.Description = request.Description;
            _innRepository.Update(inn);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<Unit> Handle(DeleteInnCommand request, CancellationToken cancellationToken)
        {
            var inn = await _innRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (inn == null)
                throw new NotFoundException($"INN with id: {request.Id} was not found");
            _innRepository.Delete(inn);
            await _unitOfWork.SaveChangesAsync();
            return Unit.Value;
        }
        private async Task ValidateName(Guid id, string name, CancellationToken cancellationToken,
            ValidationResult validationErrors)
        {
            var existingName =
                await _innRepository.Table.AnyAsync(x => x.Name == name && x.Id != id,
                    cancellationToken: cancellationToken);
            if (existingName)
                validationErrors.Errors.Add(new ValidationFailure("Code",
                    "There is an INN with the same  name, please change the  name "));
        }
    }
}
