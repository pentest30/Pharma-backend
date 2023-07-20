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

namespace GHPCommerce.Application.Catalog.PickingZones.Commands
{
    public class PickingZoneCommandHandler : 
        ICommandHandler<CreatePickingZoneCommand, ValidationResult>,
        ICommandHandler<UpdatePickingZoneCommand, ValidationResult>,
        ICommandHandler<DeletePickingZoneCommand>
    {
        private readonly IRepository<PickingZone, Guid> _pickingZoneRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PickingZoneCommandHandler(IRepository<PickingZone, Guid> pickingZoneRepository)
        {
            _pickingZoneRepository = pickingZoneRepository;
            _unitOfWork = pickingZoneRepository.UnitOfWork;
        }
        public  async Task<ValidationResult> Handle(CreatePickingZoneCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreatePickingZoneCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateName(request.Id, request.Name, cancellationToken, validationErrors);
            if (!validationErrors.IsValid)
                return validationErrors;
            var pickingZone = new  PickingZone(request.Id, request.Name,request.ZoneGroupId.Value);
            pickingZone.Order = request.Order;
            _pickingZoneRepository.Add(pickingZone);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<ValidationResult> Handle(UpdatePickingZoneCommand request, CancellationToken cancellationToken)
        {
            var pickingZone = await _pickingZoneRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (pickingZone == null)
                throw new NotFoundException($"Brand with id: {request.Id} was not found");
            var validator = new UpdatePickingZoneCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateName(request.Id, request.Name, cancellationToken, validationErrors);

            if (!validationErrors.IsValid)
                return validationErrors;
            pickingZone.Name = request.Name;
            pickingZone.Order = request.Order;
            pickingZone.ZoneType = request.ZoneType;
            pickingZone.ZoneGroupId = request.ZoneGroupId;
            _pickingZoneRepository.Update(pickingZone);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<Unit> Handle(DeletePickingZoneCommand request, CancellationToken cancellationToken)
        {
            var pickingZone = await _pickingZoneRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (pickingZone == null)
                throw new NotFoundException($"Brand with id: {request.Id} was not found");
            _pickingZoneRepository.Delete(pickingZone);
            await _unitOfWork.SaveChangesAsync();
            return Unit.Value;
        }
        private async Task ValidateName(Guid id, string name, CancellationToken cancellationToken,
            ValidationResult validationErrors)
        {
            var existingName =
                await _pickingZoneRepository.Table.AnyAsync(x => x.Name == name && x.Id != id,
                    cancellationToken: cancellationToken);
            if (existingName )
                validationErrors.Errors.Add(new ValidationFailure("Code",
                    "There is an INN code with the same  name, please change the  name "));
        }
    }
}
