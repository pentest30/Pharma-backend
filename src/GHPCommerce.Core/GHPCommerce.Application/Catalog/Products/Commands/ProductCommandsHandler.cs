using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.ValueObjects;
using GHPCommerce.Infra.OS;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace GHPCommerce.Application.Catalog.Products.Commands
{
    public class ProductCommandsHandler : 
        ICommandHandler<CreateDraftProductCommand, ValidationResult>,
        ICommandHandler<UpdateProductCommand, ValidationResult>,
        ICommandHandler<DeleteProductCommand>,
        ICommandHandler<CreateListImagesCommand>,
        ICommandHandler<ValidateProductCommand, ValidationResult>,
        ICommandHandler<AddOrUpdateAXProductCommand, ValidationResult>,
        ICommandHandler<DeleteProductByCodeCommand>,
        ICommandHandler<DisableProductCommand, ValidationResult>,
        ICommandHandler<EnableProductCommand, ValidationResult>,
        ICommandHandler<ProductHasQuotaCommand, ValidationResult>,
        ICommandHandler<ProductRemoveQuotaCommand, ValidationResult>



    {
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<Manufacturer, Guid> _manufactureRepository;
        private readonly IRepository<TaxGroup, Guid> _taxRepository;
        private readonly IRepository<ProductClass, Guid> _productClassRepository;
        private readonly IRepository<INNCode, Guid> _innCodeRepository;
        private readonly IRepository<INN, Guid> _innRepository;
        private readonly IRepository<Form, Guid> _formRepository;
        private readonly IUnitOfWork _unitOfWork;
        private SemaphoreSlim _semaphoreSlim;
        private readonly MedIJKModel _model;
        private readonly ICommandBus _commandBus;
        private readonly IRepository<PickingZone, Guid> _zoneRepository;

        public ProductCommandsHandler(IRepository<Product, Guid> productRepository, IMapper mapper,
            IRepository<Manufacturer, Guid> manufactureRepository,
            IRepository<TaxGroup, Guid> taxRepository, 
            IRepository<ProductClass, Guid> productClassRepository, 
            IRepository<INNCode, Guid> innCodeRepository, 
            IRepository<INN, Guid> innRepository,
            IRepository<Form, Guid> formRepository,
            MedIJKModel model,
            ICommandBus commandBus,
            IRepository<PickingZone, Guid> zoneRepository)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _manufactureRepository = manufactureRepository;
            _taxRepository = taxRepository;
            _productClassRepository = productClassRepository;
            _innCodeRepository = innCodeRepository;
            _innRepository = innRepository;
            _formRepository = formRepository;
            _model = model;
            _commandBus = commandBus;
            _zoneRepository = zoneRepository;
            _unitOfWork = productRepository.UnitOfWork;
            _semaphoreSlim = new SemaphoreSlim(1, 1);
        }
        public async Task<ValidationResult> Handle(CreateDraftProductCommand request, CancellationToken cancellationToken)
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            var validator = new CreateDraftProductCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateNameAndCode(request.Id, request.FullName, request.Code, cancellationToken, validationErrors);

            if (!validationErrors.IsValid)
            {
                _semaphoreSlim.Release();
                return validationErrors;
            }

            var product = _mapper.Map<Product>(request);
            _productRepository.Add(product);
            await _unitOfWork.SaveChangesAsync();
            _semaphoreSlim.Release();
            //TODO  : send drafProductCreated envent to the broker
            return default;
        }

        private async Task ValidateNameAndCode(Guid id, string name, string code, CancellationToken cancellationToken,
            ValidationResult validationErrors)
        {
            var product = await _productRepository
                .Table
                .FirstOrDefaultAsync(x => x.FullName.ToUpper().Equals(name.Trim().ToUpper()) && x.Id != id, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (product != null)
                validationErrors.Errors.Add(new ValidationFailure("Name", "ça existe dèja un produit avec ce nom. "));


            var existingProduct = await _productRepository
                .Table
                .FirstOrDefaultAsync(x => x.Code.Equals(code.Trim()) && x.Id != id, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (existingProduct == null) return;
            validationErrors.Errors.Add(new ValidationFailure("Code", "ça existe dèja un produit avec ce code. "));
        }

        public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var existingProduct = await _productRepository
                .Table
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (existingProduct == null)
                throw new NotFoundException($"Brand with id: {request.Id} was not found");

            _productRepository.Delete(existingProduct);
            await _unitOfWork.SaveChangesAsync();
            return  Unit.Value;
        
        }

        public async Task<ValidationResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);

            var existingProduct = await _productRepository
                .Table
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (existingProduct == null)
            {
                _semaphoreSlim.Release();
                throw new NotFoundException($"Product with id: {request.Id} was not found");
            }

            var validator = new UpdateProductCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateNameAndCode(request.Id, request.FullName, request.Code, cancellationToken, validationErrors);
            if (!validationErrors.IsValid)
            {
                _semaphoreSlim.Release();
                return validationErrors;
            }

            _mapper.Map(request, existingProduct);
            _productRepository.Update(existingProduct);
            await _unitOfWork.SaveChangesAsync();
            _semaphoreSlim.Release();
            return default;
        }

        public async Task<Unit> Handle(CreateListImagesCommand request, CancellationToken cancellationToken)
        {
            var mainImage = true;
            var existingProduct = await _productRepository
                .Table
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            foreach (var requestImageCommand in request.ImageCommands) {
                existingProduct.Images.Add(new ImageItem(requestImageCommand.ImageTitle, requestImageCommand.ImageBytes, (mainImage) ? true : false));
                _productRepository.Update(existingProduct);
                mainImage = false;
            }
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<ValidationResult> Handle(ValidateProductCommand request, CancellationToken cancellationToken)
        {
            var existingProduct = await _productRepository
                .Table
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (existingProduct == null)
                throw new NotFoundException($"Product with id: {request.Id} was not found"); 
            existingProduct.ProductState = ProductState.Valid;
            _productRepository.Update(existingProduct);
            await _unitOfWork.SaveChangesAsync();
            return default;

        }

        public async Task<ValidationResult> Handle(AddOrUpdateAXProductCommand request, CancellationToken cancellationToken)
        {            
            var flag = false;
            var validator = new ValidationResult();
            try
            {
                await LockProvider<string>.WaitAsync(request.Code, cancellationToken);
                var existingProduct = await _productRepository
                    .Table
                    .FirstOrDefaultAsync(x => x.Code == request.Code, cancellationToken: cancellationToken);

                if (existingProduct == null)
                {
                    existingProduct = new Product(request.FullName);
                    flag = true;
                }
                else
                    existingProduct.FullName = request.FullName;

                existingProduct.Code = request.Code;
                existingProduct.Princeps = request.Princeps;
                existingProduct.DciConcat = string.IsNullOrEmpty(request.DciConcat)?
                    existingProduct.DciConcat:request.DciConcat;
                existingProduct.Removed = false;
                existingProduct.Psychotropic = request.Psychotropic;
                existingProduct.DciCode = string.IsNullOrEmpty(request.InnCode) ?
                    existingProduct.DciCode : request.InnCode;
                existingProduct.ProductState = request.Status ? ProductState.Valid : ProductState.Deactivated;
                existingProduct.Thermolabile = request.Thermolabile;
                existingProduct.DefaultLocation =string.IsNullOrEmpty(request.DefaultLocation)?
                    existingProduct.DefaultLocation:request.DefaultLocation;
                if (!string.IsNullOrEmpty(request.PikingZone))
                {
                    var zone = await _zoneRepository.Table.FirstAsync(x => x.Name == request.PikingZone, cancellationToken: cancellationToken);
                    if (zone != null) existingProduct.PickingZoneId = zone.Id;
                }
                
                var manufacturer = default(Manufacturer);
                if (!string.IsNullOrEmpty(request.ManufacturerCode))
                    manufacturer = await _manufactureRepository.Table.FirstOrDefaultAsync(
                        x => x.Code == request.ManufacturerCode, cancellationToken: cancellationToken);
                var tax = await _taxRepository.Table.FirstOrDefaultAsync(x => x.Code == request.TaxGroupCode,
                    cancellationToken);
                //var @class =
                //    await _productClassRepository.Table.FirstOrDefaultAsync(x => x.Code == request.ProductClassCode, cancellationToken: cancellationToken);
                var innCode = await _innCodeRepository.Table.FirstOrDefaultAsync(x => x.Name == request.InnCode,
                    cancellationToken: cancellationToken);
                if (innCode != null && !string.IsNullOrWhiteSpace(request.InnCode) &&
                    !string.IsNullOrEmpty(request.InnCode))
                    existingProduct.INNCodeId = innCode.Id;
                else if (!string.IsNullOrWhiteSpace(request.InnCode) && !string.IsNullOrEmpty(request.InnCode))
                {

                    if (!string.IsNullOrEmpty(request.DciConcat))
                    {
                        var inn = await _innRepository.Table.FirstOrDefaultAsync(x => x.Name == request.DciConcat,
                            cancellationToken: cancellationToken);
                        if (inn == null)
                        {
                            _innRepository.Add(new INN { Name = request.DciConcat });
                            // await _unitOfWork.SaveChangesAsync();
                        }
                    }

                    if (!string.IsNullOrEmpty(request.FormCode))
                    {
                        var form = await _formRepository.Table.FirstOrDefaultAsync(x => x.Name == request.FormCode,
                            cancellationToken: cancellationToken);
                        var formId = Guid.NewGuid();
                        if (form == null)
                        {
                            _formRepository.Add(new Form { Id = formId, Name = request.FormCode });
                            // await _unitOfWork.SaveChangesAsync();

                        }

                        _innCodeRepository.Add(new INNCode { Name = request.InnCode, FormId = formId });
                        // await _unitOfWork.SaveChangesAsync();

                    }
                }

                if (tax == null)
                    validator.Errors.Add(new ValidationFailure("Missing tax group",
                        "le code Groupe de taxe est introuvable"));
                //if (@class == null)
                //    validator.Errors.Add(new ValidationFailure("Missing Product class", "le code Nature de produit est introuvable"));

                if (manufacturer != default)
                    existingProduct.ManufacturerId = manufacturer.Id;
                if (tax != null) existingProduct.TaxGroupId = tax.Id;
                //if (@class != null) existingProduct.ProductClassId = @class.Id;
                if (flag && validator.Errors.Count==0) _productRepository.Add(existingProduct);
                else if (validator.Errors.Count==0) _productRepository.Update(existingProduct);
                if (validator.Errors.Count==0) await _unitOfWork.SaveChangesAsync();
                //LockProvider<string>.Release(request.Code);
                return validator;
            }
            catch (Exception e)
            {
                validator.Errors.Add(new ValidationFailure("Exception", e.Message));
               //LockProvider<string>.Release(request.Code);
                return validator;
            }
            finally
            {
                LockProvider<string>.Release(request.Code);
            }
        }

        public async Task<Unit> Handle(DeleteProductByCodeCommand request, CancellationToken cancellationToken)
        {
            var existingProduct = await _productRepository
                .Table
                .FirstOrDefaultAsync(x => x.Code == request.Code, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (existingProduct == null)
                throw new NotFoundException($"Brand with id: {request.Code} was not found");

            _productRepository.Delete(existingProduct);
            await _unitOfWork.SaveChangesAsync();
            return Unit.Value;

        }

        public async Task<ValidationResult> Handle(DisableProductCommand request, CancellationToken cancellationToken)
        {
            var existingProduct = await _productRepository
               .Table
               .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken)
               .ConfigureAwait(false);
            if (existingProduct == null)
                throw new NotFoundException($"Product with id: {request.Id} was not found");
            existingProduct.ProductState = ProductState.Deactivated;
            _productRepository.Update(existingProduct);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<ValidationResult> Handle(EnableProductCommand request, CancellationToken cancellationToken)
        {
            var existingProduct = await _productRepository
               .Table
               .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken)
               .ConfigureAwait(false);
            if (existingProduct == null)
                throw new NotFoundException($"Product with id: {request.Id} was not found");
            existingProduct.ProductState = ProductState.Valid;
            _productRepository.Update(existingProduct);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }
        public async Task<ValidationResult> Handle(ProductHasQuotaCommand request, CancellationToken cancellationToken)
        {
            var existingProduct = await _productRepository
               .Table
               .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken)
               .ConfigureAwait(false);
            if (existingProduct == null)
                throw new NotFoundException($"Product with id: {request.Id} was not found");
            existingProduct.Quota = true;
            _productRepository.Update(existingProduct);
            await _unitOfWork.SaveChangesAsync();
            // if (!_model.AXInterfacing) return default;
            // var r = await _commandBus.SendAsync(new AddQuotaQuantityCommand { ProductId = request.Id }, cancellationToken);
            // if (r is { IsValid: false }) return r;
            return default;
        }
        public async Task<ValidationResult> Handle(ProductRemoveQuotaCommand request, CancellationToken cancellationToken)
        {
            var existingProduct = await _productRepository
               .Table
               .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken)
               .ConfigureAwait(false);
            if (existingProduct == null)
                throw new NotFoundException($"Product with id: {request.Id} was not found");
            if (existingProduct.Quota)
            {
                existingProduct.Quota = false;
            }
            _productRepository.Update(existingProduct);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }
    }
}
