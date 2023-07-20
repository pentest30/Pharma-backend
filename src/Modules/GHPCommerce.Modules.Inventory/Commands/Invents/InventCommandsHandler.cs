using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Commands.Invents
{
    public class InventCommandsHandler : ICommandHandler<CreateOrUpdateInventCommand, Tuple<Guid, ValidationResult>>
    {
        private readonly IRepository<Invent, Guid> _inventRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;

        public InventCommandsHandler(IRepository<Invent, Guid> inventRepository,
            ICurrentOrganization currentOrganization,
            IMapper mapper)
        {
            _inventRepository = inventRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
        }
        public async Task<Tuple<Guid, ValidationResult>> Handle(CreateOrUpdateInventCommand request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var invent =await _inventRepository.Table.FirstOrDefaultAsync(x =>
                x.OrganizationId == orgId.Value 
                && x.InternalBatchNumber == request.InternalBatchNumber 
                && x.VendorBatchNumber == request.VendorBatchNumber 
                && x.ProductId == request.ProductId, cancellationToken: cancellationToken);
            if (invent == null)
            {
                invent = _mapper.Map<Invent>(request);
                _inventRepository.Add(invent);

            }
            else
            {
                invent.PhysicalQuantity += request.PhysicalQuantity;
                _inventRepository.Update(invent);
            }

            await _inventRepository.UnitOfWork.SaveChangesAsync();
            return new Tuple<Guid, ValidationResult>(invent.Id, new ValidationResult());
        }
    }
}