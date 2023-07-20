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

namespace GHPCommerce.Application.Catalog.ZoneGroups.Commands
{
    public class ZoneGroupCommandHandler : 
        ICommandHandler<CreateZoneGroupCommand, ValidationResult>,
                ICommandHandler<UpdateZoneGroupCommand, ValidationResult>,
                ICommandHandler<DeleteZoneGroupCommand>

    {
        private readonly IRepository<ZoneGroup, Guid> _groupZoneRepository;
        private readonly IRepository<PickingZone, Guid> _pickingZoneRepository;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;


        public ZoneGroupCommandHandler(IRepository<ZoneGroup, Guid> groupZoneRepository, IRepository<PickingZone, Guid> pickingZoneRepository, IMapper mapper)
        {
            _groupZoneRepository = groupZoneRepository;
            _pickingZoneRepository = pickingZoneRepository;
            _unitOfWork = _groupZoneRepository.UnitOfWork;
            _mapper = mapper;

        }
        public async Task<ValidationResult> Handle(CreateZoneGroupCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateZoneGroupCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
                return validationErrors;
            var groupZone = new ZoneGroup(request.Id, request.Name,request.Description, request.order, request.printer);
            _groupZoneRepository.Add(groupZone);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<ValidationResult> Handle(UpdateZoneGroupCommand request, CancellationToken cancellationToken)
        {
            var zoneGroup = await _groupZoneRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id,cancellationToken: cancellationToken);
            var validator = new UpdateZoneGroupCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
                return validationErrors;
            zoneGroup.Name = request.Name;
            zoneGroup.Order = request.Order;
            zoneGroup.Description = request.Description;
            zoneGroup.Printer = request.Printer;
            _groupZoneRepository.Update(zoneGroup);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<Unit> Handle(DeleteZoneGroupCommand request, CancellationToken cancellationToken)
        {
            var zoneGroup =
              await _groupZoneRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id,
                  cancellationToken: cancellationToken);
            if (zoneGroup == null)
                throw new NotFoundException($"Zone group with id: {request.Id} was not found");
            var hasZones = await _pickingZoneRepository
                .Table
                .AnyAsync(x => x.ZoneGroupId.Value == request.Id, cancellationToken: cancellationToken);
            if (hasZones)
                throw new InvalidOperationException("You cannot remove this zone group");
            _groupZoneRepository.Delete(zoneGroup);
            await _unitOfWork.SaveChangesAsync();
            return Unit.Value;
        }
    }
}
