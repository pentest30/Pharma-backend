using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.Entities;
using GHPCommerce.Modules.Sales.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class DiscountsCommandHandler :
        ICommandHandler<CreateDiscountCommand, ValidationResult>,
        ICommandHandler<UpdateDiscountCommand, ValidationResult>, 
        ICommandHandler<CreateDiscountAxCommand, ValidationResult>
    {
        private readonly IDiscountsRepository _discountsRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly ICache _redisCache;

        public DiscountsCommandHandler(
            IDiscountsRepository discountsRepository,
            IMapper mapper,
            ICommandBus commandBus,
            ICurrentOrganization currentOrganization,
            ICurrentUser currentUser,
            ICache redisCache)
        {
            _discountsRepository = discountsRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _redisCache = redisCache;
            _commandBus = commandBus;

        }
        public async Task<ValidationResult> Handle(CreateDiscountCommand request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var discountExists = await _discountsRepository.Table.FirstOrDefaultAsync(x => x.ProductId == request.ProductId 
            && x.ThresholdQuantity == request.ThresholdQuantity
            && (request.from < x.to && x.from < request.to),
                 cancellationToken: cancellationToken);
            if (discountExists != null)
            {
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Discount of product", $"Discount of product : {request.ProductFullName} is already existing in the date range from : {request.from.ToShortDateString()} to : {request.to.ToShortDateString()} ")
                    }
                };
               
            }
            var validator = new CreateDiscountCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
                return validationErrors;
            var discount = _mapper.Map<Discount>(request);
            discount.OrganizationId = (Guid)orgId;
            _discountsRepository.Add(discount);
            await _discountsRepository.UnitOfWork.SaveChangesAsync();
            return default;
        }
        public async Task<ValidationResult> Handle(UpdateDiscountCommand request, CancellationToken cancellationToken)
        {
            var discount =  await _discountsRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id,
                 cancellationToken: cancellationToken);
            if (discount == null)
                throw new NotFoundException($"Discount with id: {request.Id} was not found");
            var discountExists = await _discountsRepository.Table
                .FirstOrDefaultAsync(x =>x.Id != request.Id && x.ProductId == request.ProductId
            && x.ThresholdQuantity == request.ThresholdQuantity
            && (request.from < x.to && x.from < request.to),
                 cancellationToken: cancellationToken);
            if (discountExists != null)
            {
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Discount of product", $"Discount of product : {request.ProductFullName} is already existing in the date range from : {request.from.ToShortDateString()} to : {request.to.ToShortDateString()} ")
                    }
                };

            }
            var validator = new CreateDiscountCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
                return validationErrors;
            discount.ProductId = request.ProductId;
            discount.ProductFullName = request.ProductFullName;
            discount.ThresholdQuantity = request.ThresholdQuantity;
            discount.DiscountRate = request.DiscountRate;
            discount.from = request.from;
            discount.to = request.to;

            _discountsRepository.Update(discount);
            await _discountsRepository.UnitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<ValidationResult> Handle(CreateDiscountAxCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateDiscountAxCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
                return validationErrors;
            var product = await _commandBus.SendAsync(new GetProductByCode { CodeProduct = request.ProductCode }, cancellationToken);
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null)
            {
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Missing Organization", "Vous n'etes pas autorisé d'acceder à cette ressource !")
                    }
                };
            }
            if (product == null)
            {
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Missing Product code", "Code Article introuvable !")
                    }
                };
            }
            var discount = await _discountsRepository.Table
                .FirstOrDefaultAsync(x => x.ProductId == product.Id
                                          &&  x.ThresholdQuantity == request.ThresholdQuantity
                    && x.OrganizationId == orgId.Value,
                cancellationToken: cancellationToken);

            if (discount != null)
            {
                _discountsRepository.Delete(discount);
            }
            discount = new Discount();
            discount.ThresholdQuantity = request.ThresholdQuantity;
            discount.DiscountRate = request.DiscountRate;
            discount.ProductFullName = product.FullName;
            discount.ProductId = product.Id;
            discount.from = request.From;
            discount.to = request.To;
            discount.OrganizationId = orgId.Value;
            _discountsRepository.Add(discount);
            await _discountsRepository.UnitOfWork.SaveChangesAsync();
            return default;
        }
    }
}
