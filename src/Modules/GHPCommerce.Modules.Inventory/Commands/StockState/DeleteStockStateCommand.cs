using AutoMapper;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Modules.Inventory.Commands.StockState
{
    public class DeleteStockStateCommand : ICommand
    {
        public Guid Id { get; set; }
    }
    public class DeleteStockStateCommandHandler : ICommandHandler<DeleteStockStateCommand>
    {
        private readonly IRepository<Entities.StockState, Guid> _stockStateRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;


        public DeleteStockStateCommandHandler(
            IRepository<Entities.StockState, Guid> stockStateRepository,
            IMapper mapper,
            ICurrentOrganization currentOrganization)
        {
            _stockStateRepository = stockStateRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
        }
        public async Task<Unit> Handle(DeleteStockStateCommand request, CancellationToken cancellationToken)
        {
            // var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var existingZoneType =
                await _stockStateRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id,
                    cancellationToken: cancellationToken);
            if (existingZoneType == null) throw new NotFoundException("Stock state type with id " + request.Id + " was not found");
            _stockStateRepository.Delete(existingZoneType);
            await _stockStateRepository.UnitOfWork.SaveChangesAsync();
            return default;
        }

    }
}
