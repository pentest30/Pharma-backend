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

namespace GHPCommerce.Application.Catalog.ProductClasses.Commands
{
    public class ProductClassCommandsHandler :
        ICommandHandler<CreateProductClassCommand, ValidationResult>,
        ICommandHandler<UpdateProductClassCommand, ValidationResult>,
        ICommandHandler<DeleteProductClassCommand>
    {
        private readonly IRepository<ProductClass, Guid> _productClassRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        public ProductClassCommandsHandler(IRepository<ProductClass, Guid> productClassRepository, IRepository<Product, Guid> productRepository)
        {
            _productClassRepository = productClassRepository;
            _productRepository = productRepository;
            _unitOfWork = productClassRepository.UnitOfWork;
        }

        public async Task<ValidationResult> Handle(CreateProductClassCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateProductClassModelValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            request.Id = Guid.NewGuid();
            await ValidateName(request.Id, request.Name, cancellationToken, validationErrors);
            if (!validationErrors.IsValid)
                return validationErrors;

            var productClass = new ProductClass(request.Id,  request.Name, request.Description, request.ParentProductClassId, request.IsMedicamentClass);
            productClass.Code = request.Code;
            _productClassRepository.Add(productClass);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }


        public async Task<ValidationResult> Handle(UpdateProductClassCommand request, CancellationToken cancellationToken)
        {
            var productClass = await _productClassRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if(productClass == null )
                throw  new NotFoundException($"Product class with code: {request.Id} was not found" );
            var validator = new UpdateProductClassModelValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateName(productClass.Id, request.Name, cancellationToken, validationErrors);

            if (!validationErrors.IsValid)
                return validationErrors;
            productClass.Description = request.Description;
            productClass.Name = request.Name;
            productClass.ParentProductClassId = request.ParentProductClassId;
            productClass.IsMedicamentClass = request.IsMedicamentClass;
            _productClassRepository.Update(productClass);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<Unit> Handle(DeleteProductClassCommand request, CancellationToken cancellationToken)
        {
            var productClass = await _productClassRepository
                .Table
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
           
            if (productClass == null)
                throw new NotFoundException($"Product class with id: {request.Id} was not found");
           
            var hasProducts = await _productRepository
                .Table
                .AnyAsync(x => x.ProductClassId == request.Id, cancellationToken: cancellationToken);
           if(hasProducts)
               throw new InvalidOperationException("You cannot remove this class of products");
           
           _productClassRepository.Delete(productClass);
            await _unitOfWork.SaveChangesAsync();
            return  Unit.Value;
        }
        private async Task ValidateName(Guid id, string name, CancellationToken cancellationToken,
            ValidationResult validationErrors)
        {
            var manufacturerName = await _productClassRepository
                .Table
                .AnyAsync(x => x.Name.Equals(name.Trim()) && x.Id != id, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (manufacturerName )
                validationErrors.Errors.Add(new ValidationFailure("Name", "ça existe dèja une classe avec ce nom. "));

        }
    }
}