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

namespace GHPCommerce.Application.Catalog.Dosages.Commands
{
    public class DosageCommandsHandler : 
        ICommandHandler<CreateDosageCommand, ValidationResult>,
        ICommandHandler<UpdateDosageCommand, ValidationResult>,
        ICommandHandler<DeleteDosageCommand>
    {
        private readonly IRepository<Dosage, Guid> _dosageRepository;
        private readonly IUnitOfWork _unitOfWork;
        public DosageCommandsHandler(IRepository<Dosage, Guid> dosageRepository)
        {
            _dosageRepository = dosageRepository;
            _unitOfWork = dosageRepository.UnitOfWork;
        }
        public async Task<ValidationResult> Handle(CreateDosageCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateDosageCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateName(request.Id,request.Name, cancellationToken, validationErrors);
            if (!validationErrors.IsValid)
                return validationErrors;
            var dosage = new Dosage(request.Id, request.Name.ToUpper());
            _dosageRepository.Add(dosage);
            await _unitOfWork.SaveChangesAsync();
            return default;

        }

        public async Task<ValidationResult> Handle(UpdateDosageCommand request, CancellationToken cancellationToken)
        {
            var dosage = await _dosageRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id,
                cancellationToken: cancellationToken);
            if (dosage == null)
                throw new NotFoundException($"Dosage with id: {request.Id} was not found");
            var validator = new UpdateDosageCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateName(request.Id, request.Name, cancellationToken, validationErrors);

            if (!validationErrors.IsValid)
                return validationErrors;
            dosage.Name =request.Name.ToUpper();
            _dosageRepository.Update(dosage);
            await _unitOfWork.SaveChangesAsync();
            return default;

        }
        private async Task ValidateName(Guid id ,string name, CancellationToken cancellationToken,
            ValidationResult validationErrors)
        {
            var existingForm =
                await _dosageRepository.Table.AnyAsync(x => x.Name == name && x.Id!=id,
                    cancellationToken: cancellationToken);
            if (existingForm )
                validationErrors.Errors.Add(new ValidationFailure("Code",
                    "There is a form with the same  name, please change the  name "));
        }
        public async Task<Unit> Handle(DeleteDosageCommand request, CancellationToken cancellationToken)
        {
            var dosage = await _dosageRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id,
                cancellationToken: cancellationToken);
            if (dosage == null)
                throw new NotFoundException($"Dosage with id: {request.Id} was not found");
          
            _dosageRepository.Delete(dosage);
            await _unitOfWork.SaveChangesAsync();
            return Unit.Value;
        }
    }
}
