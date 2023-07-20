using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Application.Tiers.Suppliers.Events;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Tiers.Suppliers.Commands
{
    public class SuppliersCommandsHandler :ICommandHandler<CreateSupplierCommand, ValidationResult>
    {
        private readonly IRepository<Supplier, Guid> _supplierRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly ICurrentOrganization _currentOrganization;

        public SuppliersCommandsHandler(IRepository<Supplier, Guid> supplierRepository, IMapper mapper,IMediator mediator,ICurrentOrganization currentOrganization)
        {
            _supplierRepository = supplierRepository;
            _mapper = mapper;
            _unitOfWork = _supplierRepository.UnitOfWork;
            _mediator = mediator;
            _currentOrganization = currentOrganization;
        }
        public async Task<ValidationResult> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
        {
            var organizationId =await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!organizationId.HasValue && request.IsCustomer)
                return new ValidationResult {Errors = {new ValidationFailure("OrganizationId", "Utilisateur non affecté à une organization")}};
            var currentOrg = request.IsCustomer ? organizationId : request.OrganizationId;
            var supplier = await _supplierRepository.Table.FirstOrDefaultAsync(x => x.OrganizationId == currentOrg.Value, cancellationToken);
            if (supplier == null)
            {
                Guid id = Guid.NewGuid();
                supplier = new Supplier { OrganizationId = currentOrg.Value, Id = id };
                _supplierRepository.Add(supplier);
                await _unitOfWork.SaveChangesAsync();
            }
            var notification = _mapper.Map<SupplierCreatedEvent>(request);
            notification.IsSupplier = !request.IsCustomer;
            notification.SupplierId = supplier.Id;
            await _mediator.Publish(notification, cancellationToken);
             
            return default;
        }
    }
}
