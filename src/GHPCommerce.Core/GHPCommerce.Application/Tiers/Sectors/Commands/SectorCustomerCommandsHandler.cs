using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Tiers.Sectors.Commands
{
    public class SectorCustomerCommandsHandler : 
        ICommandHandler<CreateSectorCommand, ValidationResult>,
        ICommandHandler<UpdateSectorCommand, ValidationResult>, 
        ICommandHandler<DeleteSectorCommand>
    {
        private readonly IMapper _mapper;
        private readonly IRepository<SectorCustomer, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;

        public SectorCustomerCommandsHandler(IMapper mapper, IRepository<SectorCustomer, Guid> repository, ICurrentOrganization currentOrganization)
        {
            _mapper = mapper;
            _repository = repository;
            _currentOrganization = currentOrganization;
        }
        public async Task<ValidationResult> Handle(CreateSectorCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (orgId == null)
                    throw new InvalidOperationException("You cannot use this resource");
                var sectorCustomer =await _repository.Table.FirstOrDefaultAsync(x => x.Code == request.Code, cancellationToken: cancellationToken);
                if (sectorCustomer == null)
                {
                    sectorCustomer = _mapper.Map<SectorCustomer>(request);
                    sectorCustomer.OrganizationId = orgId.Value;
                    _repository.Add(sectorCustomer);
                }
                else
                {
                    sectorCustomer.Name = request.Name;
                    _repository.Update(sectorCustomer);
                }
                await _repository.UnitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Removing order error", e.Message)
                    }
                };
            }
            return default;
        }

        public async Task<ValidationResult> Handle(UpdateSectorCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.Table.FirstOrDefaultAsync(x => x.Code == request.Code, cancellationToken);
            if(entity == null) 
                throw  new NotFoundException($"Sector with id {request.Code} was not found");
            entity.Name = request.Name;
            entity.Code = request.Code;
            //entity.OrganizationId = request.OrganizationId;
            _repository.Update(entity);
            await _repository.UnitOfWork.SaveChangesAsync();
            return default;

        }

        public async Task<Unit> Handle(DeleteSectorCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null)
                throw new NotFoundException($"Sector with id {request.Id} was not found");
            _repository.Delete(entity);
            await _repository.UnitOfWork.SaveChangesAsync();
            return default;

        }
    }
}
