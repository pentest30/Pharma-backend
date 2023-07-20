using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GHPCommerce.Modules.Inventory.Commands.TransferLogs
{
    public class TransferLogUpdateCommand : TransferLogCreateCommand
    {
    }
    public class TransferLogUpdateCommandHandler : ICommandHandler<TransferLogUpdateCommand, ValidationResult>
    {
        private readonly IRepository<TransferLog, Guid> _transferRepository;
        private readonly IRepository<Invent, Guid> _inventRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly Logger _logger;

        public TransferLogUpdateCommandHandler(IRepository<TransferLog, Guid> transferRepository,
            IRepository<Invent, Guid> inventRepository,
            ICurrentOrganization currentOrganization,
            IMapper mapper, 
            Logger logger)
        {
            _transferRepository = transferRepository;
            _inventRepository = inventRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<ValidationResult> Handle(TransferLogUpdateCommand request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == default) throw new InvalidOperationException("Resources not allowed");
            ValidationResult validations = default;

            try
            {
                var invent = await _inventRepository.Table
                    .FirstOrDefaultAsync(x => x.ZoneId == request.ZoneSourceId
                                              && x.InternalBatchNumber == request.InternalBatchNumber 
                                              && x.ProductId == request.ProductId
                                              && x.OrganizationId == orgId, cancellationToken: cancellationToken);
                if (invent == null || invent.PhysicalQuantity < request.Quantity)
                    throw new InvalidOperationException("Quantité non disponible");
                var transferLog = await _transferRepository.Table
                    .Include(x=>x.Items)
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
                if (transferLog == null)
                    throw new NotFoundException($"Transfer log with  {request.Id} was not found");
                var line = transferLog.Items
                    .FindIndex(x => x.ProductId == request.ProductId 
                                    && x.InternalBatchNumber == request.InternalBatchNumber);
                if (line == -1) throw new NotFoundException($"line  was not found");
                transferLog.ZoneDestId = request.ZoneDestId;
                transferLog.ZoneSourceId = request.ZoneSourceId;
                transferLog.StockStateId = request.StockStateId;
                transferLog.ZoneDestName = request.ZoneDestName;
                transferLog.ZoneSourceName = request.ZoneSourceName;
                transferLog.StockStateName = request.StockStateName;
                transferLog.Items[line].Quantity = request.Quantity;
                _transferRepository.Update(transferLog);
                await _transferRepository.UnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                validations = new ValidationResult
                    {Errors = {new ValidationFailure("Transaction rolled back", ex.Message)}};
                _logger.Error(ex.Message);
                _logger.Error(ex.InnerException?.Message);
            }

            return validations;
        }
    }
}