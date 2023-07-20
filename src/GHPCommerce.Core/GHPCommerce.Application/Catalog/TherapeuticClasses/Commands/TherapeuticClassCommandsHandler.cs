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

namespace GHPCommerce.Application.Catalog.TherapeuticClasses.Commands
{
    public class TherapeuticClassCommandsHandler : 
        ICommandHandler<CreateTherapeuticClassCommand, ValidationResult>,
        ICommandHandler<UpdateTherapeuticClassCommand, ValidationResult>,
        ICommandHandler<DeleteTherapeuticClassCommand>
    {
        private readonly IRepository<TherapeuticClass, Guid> _therapeuticClassRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Product, Guid> _productRepository;

        public TherapeuticClassCommandsHandler(IRepository<TherapeuticClass, Guid> therapeuticClassRepository, IRepository<Product, Guid> productRepository)
        {
            _therapeuticClassRepository = therapeuticClassRepository;
            _productRepository = productRepository;
            _unitOfWork = _therapeuticClassRepository.UnitOfWork;
        }
        public async Task<ValidationResult> Handle(CreateTherapeuticClassCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateTherapeuticClassCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
                return validationErrors;
            var therapeuticCLass = new TherapeuticClass(request.Id,request.Name, request.Description);
            _therapeuticClassRepository.Add(therapeuticCLass);
            await _unitOfWork.SaveChangesAsync();
            return default;

        }

        public async Task<ValidationResult> Handle(UpdateTherapeuticClassCommand request, CancellationToken cancellationToken)
        {
            var therapeuticCLass = await _therapeuticClassRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if(therapeuticCLass == null) 
                throw new  NotFoundException($"therapeutic cLass with id: {request.Id} was not found");
            therapeuticCLass.Name = request.Name;
            therapeuticCLass.Description = request.Description;
            _therapeuticClassRepository.Update(therapeuticCLass);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<Unit> Handle(DeleteTherapeuticClassCommand request, CancellationToken cancellationToken)
        {
            var therapeuticCLass = await _therapeuticClassRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (therapeuticCLass == null)
                throw new NotFoundException($"therapeutic cLass with id: {request.Id} was not found");
            var hasProducts = await _productRepository
                .Table
                .AnyAsync(x => x.ProductClassId == request.Id, cancellationToken: cancellationToken);
            if (hasProducts)
                throw new InvalidOperationException("You cannot remove this Item");
            _therapeuticClassRepository.Delete(therapeuticCLass);
            await _unitOfWork.SaveChangesAsync();
            return Unit.Value;
        }
    }
}
