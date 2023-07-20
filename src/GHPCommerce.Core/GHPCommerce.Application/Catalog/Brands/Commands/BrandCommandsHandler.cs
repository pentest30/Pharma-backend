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

namespace GHPCommerce.Application.Catalog.Brands.Commands
{
    public class BrandCommandsHandler : 
        ICommandHandler<CreateBrandCommand, ValidationResult>,
        ICommandHandler<UpdateBrandCommand, ValidationResult>,
        ICommandHandler<DeleteBrandCommand>
    {
        private readonly IRepository<Brand, Guid> _brandRepository;
        private readonly IUnitOfWork _unitOfWork;
        public BrandCommandsHandler(IRepository<Brand, Guid> brandRepository)
        {
            _brandRepository = brandRepository;
             _unitOfWork = brandRepository.UnitOfWork;
        }
        public  async Task<ValidationResult> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateBrandCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateName(request.Id, request.Name, cancellationToken, validationErrors);
            if (!validationErrors.IsValid)
                return validationErrors;
            var brand = new Brand(request.Id, request.Name, request.Description);
            _brandRepository.Add(brand);
            await _unitOfWork.SaveChangesAsync();
            return default;

        }

        public async Task<ValidationResult> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
        {
            var brand = await _brandRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (brand == null)
                throw new NotFoundException($"Brand with id: {request.Id} was not found");
            var validator = new UpdateBrandCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateName(request.Id, request.Name, cancellationToken, validationErrors);
            if (!validationErrors.IsValid)
                return validationErrors;
            brand.Name = request.Name;
            brand.Description = request.Description;

            _brandRepository.Update(brand);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<Unit> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
        {
            var brand = await _brandRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (brand == null)
                throw new NotFoundException($"Brand with id: {request.Id} was not found");
            _brandRepository.Delete(brand);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }
        private async Task ValidateName(Guid id, string name, CancellationToken cancellationToken, ValidationResult validationErrors)
        {
            var existingForm =
                await _brandRepository.Table.AnyAsync(x => x.Name == name && x.Id != id,
                    cancellationToken: cancellationToken);
            if (existingForm )
                validationErrors.Errors.Add(new ValidationFailure("Code",
                    "There is a brand with the same  name, please change the  name "));
        }
    }
}
