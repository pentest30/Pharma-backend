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

namespace GHPCommerce.Application.Catalog.Packaging.Commands
{
    public class PackagingCommandsHandler : 
        ICommandHandler<CreatePackagingCommand, ValidationResult> ,
        ICommandHandler<UpdatePackagingCommand, ValidationResult>,
        ICommandHandler<DeletePackagingCommand>
    {
        private readonly IRepository<Domain.Domain.Catalog.Packaging, Guid> _packagingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Product, Guid> _productRepository;

        public PackagingCommandsHandler(IRepository<Domain.Domain.Catalog.Packaging , Guid> packagingRepository, IRepository<Product, Guid> productRepository)
        {
            _packagingRepository = packagingRepository;
            _productRepository = productRepository;
            _unitOfWork = packagingRepository.UnitOfWork;
        }
        public async Task<ValidationResult> Handle(CreatePackagingCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreatePackagingCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateName(request.Id, request.Name, cancellationToken, validationErrors);
            if (!validationErrors.IsValid)
                return validationErrors;
            var packageExist = await _packagingRepository.Table.AnyAsync(x => x.Code.ToLower() == request.Code.ToLower() || x.Name.ToLower()== request.Name.ToLower(), cancellationToken: cancellationToken);
            if (packageExist)
            {
                validationErrors.Errors.Add(new ValidationFailure("Code", "ça existe un nom / code pareil dans la base de donées. Veuillez changer le nom / code "));
                return validationErrors;
            }
            var package = new Domain.Domain.Catalog.Packaging(request.Id,request.Code, request.Name);
            _packagingRepository.Add(package);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<ValidationResult> Handle(UpdatePackagingCommand request, CancellationToken cancellationToken)
        {
            var packaging = await _packagingRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if(packaging == null)
                throw  new NotFoundException($"Packaging with id: {request.Id} was not found");
            var validator = new UpdatePackagingCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateName(request.Id, request.Name, cancellationToken, validationErrors);
            if (!validationErrors.IsValid)
                return validationErrors;
            packaging.Code = request.Code;
            packaging.Name = request.Name;
            _packagingRepository.Update(packaging);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<Unit> Handle(DeletePackagingCommand request, CancellationToken cancellationToken)
        {
            var packaging = await _packagingRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (packaging == null)
                throw new NotFoundException($"Packaging with id: {request.Id} was not found");
            var hasProducts = await _productRepository
                .Table
                .AnyAsync(x => x.ProductClassId == request.Id, cancellationToken: cancellationToken);
            if (hasProducts)
                throw new InvalidOperationException("You cannot remove this item");
            _packagingRepository.Delete(packaging);
            await _unitOfWork.SaveChangesAsync();
            return Unit.Value;
        }
        private async Task ValidateName(Guid id, string name, CancellationToken cancellationToken, ValidationResult validationErrors)
        {
            var existingForm =
                await _packagingRepository.Table.AnyAsync(x => x.Name == name && x.Id != id,
                    cancellationToken: cancellationToken);
            if (existingForm)
                validationErrors.Errors.Add(new ValidationFailure("Code",
                    "There is a packaging set with the same  name, please change the  name "));
        }
    }
}
