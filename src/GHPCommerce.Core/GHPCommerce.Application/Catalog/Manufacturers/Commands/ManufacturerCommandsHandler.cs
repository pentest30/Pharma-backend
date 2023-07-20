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

namespace GHPCommerce.Application.Catalog.Manufacturers.Commands
{
    public class ManufacturerCommandsHandler :
        ICommandHandler<CreateManufacturerCommand, ValidationResult>,
        ICommandHandler<UpdateManufacturerCommand, ValidationResult>,
        ICommandHandler<UpdateManufacturerByCodeCommand, ValidationResult>,
        ICommandHandler<DeleteManufacturerCommand>,
        ICommandHandler<DeleteManufacturerByCodeCommand>
    {
        private readonly IRepository<Manufacturer, Guid> _manufactureRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ManufacturerCommandsHandler(IRepository<Manufacturer, Guid> manufactureRepository,  IRepository<Product, Guid> productRepository)
        {
            _manufactureRepository = manufactureRepository;
            _productRepository = productRepository;
            _unitOfWork = manufactureRepository.UnitOfWork;
        }
        public async Task<ValidationResult> Handle(CreateManufacturerCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateManufacturerCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
                return validationErrors;
            var manufacturer = await _manufactureRepository.Table.FirstOrDefaultAsync(x => x.Code == request.Code, cancellationToken: cancellationToken);

            if (manufacturer == null)
            {
                manufacturer = new Manufacturer(request.Code, request.Name, request.Addresses, request.PhoneNumbers, request.Emails);
                _manufactureRepository.Add(manufacturer);
            }
            else
            {
                manufacturer.Name = request.Name;
                _manufactureRepository.Update(manufacturer);
            }
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

       

        public async Task<ValidationResult> Handle(UpdateManufacturerCommand request, CancellationToken cancellationToken)
        {
            var manufacturer =await _manufactureRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (manufacturer == null)
                throw new NotFoundException($"manufacturer with id: {request.Id} was not found");
            var validator = new UpdateManufacturerCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateNameAndCode(request.Id, request.Name, request.Code, cancellationToken, validationErrors);

            if (!validationErrors.IsValid)
                return validationErrors;

            manufacturer .UpdateManufacturer( request.Code, request.Name, request.Addresses, request.PhoneNumbers , request.Emails);
            _manufactureRepository.Update(manufacturer);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<Unit> Handle(DeleteManufacturerCommand request, CancellationToken cancellationToken)
        {
            var manufacturer = await _manufactureRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (manufacturer == null)
                throw new NotFoundException($"manufacturer with id: {request.Id} was not found");
            var hasProducts = await _productRepository
                .Table
                .AnyAsync(x => x.ProductClassId == request.Id, cancellationToken: cancellationToken);
            if (hasProducts)
                throw new InvalidOperationException("You cannot remove this class of products");

            _manufactureRepository.Delete(manufacturer);
            await _unitOfWork.SaveChangesAsync();
            return  Unit.Value;
        }
       
        private async Task ValidateNameAndCode(Guid id ,string name , string code, CancellationToken cancellationToken,
            ValidationResult validationErrors)
        {
            var existingManufacturerName = await _manufactureRepository
                .Table
                .AnyAsync(x => x.Name.Equals(name) && x.Id !=id, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            
            if (existingManufacturerName)
                validationErrors.Errors.Add(new ValidationFailure("Name", "ça existe dèja un Fabriquant avec ce nom. "));


            var existingManufacturerCode = await _manufactureRepository
                .Table
                .AnyAsync(x => x.Code.Equals(code) && x.Id != id, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            
            if (!existingManufacturerCode) return;
            validationErrors.Errors.Add(new ValidationFailure("Code", "ça existe dèja un Fabriquant avec ce code. "));
        }

        public async Task<ValidationResult> Handle(UpdateManufacturerByCodeCommand request, CancellationToken cancellationToken)
        {
            var manufacturer = await _manufactureRepository.Table.FirstOrDefaultAsync(x => x.Code == request.Code, cancellationToken: cancellationToken );
            if (manufacturer == null)
                throw new NotFoundException($"manufacturer with code: {request.Code} was not found");
            manufacturer.UpdateManufacturer(request.Code, request.Name, request.Addresses, request.PhoneNumbers, request.Emails);
            _manufactureRepository.Update(manufacturer);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<Unit> Handle(DeleteManufacturerByCodeCommand request, CancellationToken cancellationToken)
        {
            var manufacturer = await _manufactureRepository.Table.FirstOrDefaultAsync(x => x.Code == request.Code, cancellationToken: cancellationToken);
            if (manufacturer == null)
                throw new NotFoundException($"manufacturer with code: {request.Code} was not found");
            var hasProducts = await _productRepository
                .Table
                .AnyAsync(x => x.ProductClassId == manufacturer.Id, cancellationToken: cancellationToken);
            if (hasProducts)
                throw new InvalidOperationException("You cannot remove this class of products");

            _manufactureRepository.Delete(manufacturer);
            await _unitOfWork.SaveChangesAsync();
            return Unit.Value;
        }
    }
}
