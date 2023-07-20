using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Catalog.TaxGroups.Commands
{
    public class TaxGroupCommandsHandler : 
        ICommandHandler<CreateTaxGroupCommand, ValidationResult>,
        ICommandHandler<UpdateTaxGroupCommand, ValidationResult>,
        ICommandHandler<DeleteTaxGroupCommand>
    {
        private readonly IRepository<TaxGroup, Guid> _taxGroupRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        public TaxGroupCommandsHandler(IRepository<TaxGroup, Guid> taxGroupRepository, IRepository<Product, Guid> productRepository)
        {
            _taxGroupRepository = taxGroupRepository;
            _productRepository = productRepository;
            _unitOfWork = taxGroupRepository.UnitOfWork;
        }
        public async Task<ValidationResult> Handle(CreateTaxGroupCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateTaxGroupCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
                return validationErrors;
            var existingTaxGroup = await _taxGroupRepository.Table
                .OrderByDescending(x=>x.ValidFrom)
                .FirstOrDefaultAsync(x => x.Code == request.Code  && x.ValidFrom.Date <= request.ValidFrom.Date  , cancellationToken: cancellationToken);
            if (existingTaxGroup != null  && existingTaxGroup.ValidFrom.Date == request.ValidFrom.Date)
            {
                existingTaxGroup.Name = request.Name;
                existingTaxGroup.TaxValue = request.TaxValue;
                existingTaxGroup.ValidFrom = request.ValidFrom.Date;
                if (request.ValidTo != null) existingTaxGroup.ValidTo = request.ValidTo.Value.Date;
                _taxGroupRepository.Update(existingTaxGroup);
                await _unitOfWork.SaveChangesAsync();
                return default;
            }

            if (existingTaxGroup != null && existingTaxGroup.ValidFrom.Date < request.ValidFrom.Date && !existingTaxGroup.ValidTo.HasValue) 
            {
                existingTaxGroup.ValidTo = request.ValidFrom.Date;
                _taxGroupRepository.Update(existingTaxGroup);
                await _unitOfWork.SaveChangesAsync();
                
            }
            var taxGroup = new TaxGroup(request.Code, request.Name , request.TaxValue , request.ValidFrom, request.ValidTo);
            _taxGroupRepository.Add(taxGroup);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<ValidationResult> Handle(UpdateTaxGroupCommand request, CancellationToken cancellationToken)
        {
            var taxGroup =
                await _taxGroupRepository.Table.FirstOrDefaultAsync(x => x.Code == request.Code,
                    cancellationToken: cancellationToken);
            if (taxGroup == null) 
                throw new NotFoundException($"Tax group with code: {request.Code} was not found");
            var validator = new UpdateTaxGroupCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
                return validationErrors;
            taxGroup.Code = request.Code;
            taxGroup.Name = request.Name;
            taxGroup.TaxValue = request.TaxValue;
            taxGroup.ValidFrom = request.ValidFrom;
            taxGroup.ValidTo = request.ValidTo;
            _taxGroupRepository.Update(taxGroup);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<Unit> Handle(DeleteTaxGroupCommand request, CancellationToken cancellationToken)
        {
            var taxGroup =
                await _taxGroupRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id,
                    cancellationToken: cancellationToken);
            if (taxGroup == null)
                throw new NotFoundException($"Tax group with id: {request.Id} was not found");
            var hasProducts = await _productRepository
                .Table
                .AnyAsync(x => x.TaxGroupId== request.Id, cancellationToken: cancellationToken);
            if (hasProducts)
                throw new InvalidOperationException("You cannot remove this tax group");
            _taxGroupRepository.Delete(taxGroup);
            await _unitOfWork.SaveChangesAsync();
            return Unit.Value;
        }
    }
}
