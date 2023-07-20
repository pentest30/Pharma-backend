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

namespace GHPCommerce.Application.Catalog.PharmacologicalClasses.Commands
{
    public class PharmacologicalClassCommandsHandler :
        ICommandHandler<CreatePharmacologicalClassCommand, ValidationResult>,
        ICommandHandler<UpdatePharmacologicalClassCommand, ValidationResult>,
        ICommandHandler<DeletePharmacologicalClassCommand>
    {
        private readonly IRepository<PharmacologicalClass, Guid> _pharmacologicalClassRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Product, Guid> _productRepository;

        public PharmacologicalClassCommandsHandler(IRepository<PharmacologicalClass,Guid> pharmacologicalClassRepository, IRepository<Product, Guid> productRepository)
        {
            _pharmacologicalClassRepository = pharmacologicalClassRepository;
            _productRepository = productRepository;
            _unitOfWork = pharmacologicalClassRepository.UnitOfWork;
        }
        public async Task<ValidationResult> Handle(CreatePharmacologicalClassCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreatePharmacologicalClassCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
                return validationErrors;
            var pharmacologicalClass = new PharmacologicalClass(request.Id,request.Name, request.Description);
            _pharmacologicalClassRepository.Add(pharmacologicalClass);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<ValidationResult> Handle(UpdatePharmacologicalClassCommand request, CancellationToken cancellationToken)
        {
            var pharmacologicalClass = await _pharmacologicalClassRepository
                    .Table
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (pharmacologicalClass == null)
                throw new NotFoundException($"pharmacological Class with id: {request.Id} was not found");
            pharmacologicalClass.Name = request.Name;
            pharmacologicalClass.Description = request.Description;
            _pharmacologicalClassRepository.Update(pharmacologicalClass);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<Unit> Handle(DeletePharmacologicalClassCommand request, CancellationToken cancellationToken)
        {
            var pharmacologicalClass = await _pharmacologicalClassRepository
                .Table
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (pharmacologicalClass == null)
                throw new NotFoundException($"pharmacological Class with id: {request.Id} was not found");
            var hasProducts = await _productRepository
                .Table
                .AnyAsync(x => x.ProductClassId == request.Id, cancellationToken: cancellationToken);
            if (hasProducts)
                throw new InvalidOperationException("You cannot remove this Item");

            _pharmacologicalClassRepository.Delete(pharmacologicalClass);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }
    }
}
