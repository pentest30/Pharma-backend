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
    public class DeleteTransferLogItemCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string InternalBatchNumber { get; set; }
    }
    public class DeleteTransferLogItemCommandHandler : ICommandHandler<DeleteTransferLogItemCommand, ValidationResult>
    {
        private readonly IRepository<TransferLog, Guid> _transferRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly Logger _logger;

        public DeleteTransferLogItemCommandHandler(IRepository<TransferLog, Guid> transferRepository,  
            ICurrentOrganization currentOrganization,
            IMapper mapper,
            Logger logger)
        {
            _transferRepository = transferRepository;
            _currentOrganization = currentOrganization;
            _logger = logger;
        }
        public async Task<ValidationResult> Handle(DeleteTransferLogItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (orgId == null)
                    throw new InvalidOperationException($"resources not allowed");

                var invoice = await _transferRepository
                    .Table
                    .Include(x=>x.Items)
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
                if (invoice == null) throw new NotFoundException($"transfer log with id {request.Id} was not found");
                var index = invoice.Items
                    .FindIndex(x =>
                    x.ProductId == request.ProductId 
                    && x.InternalBatchNumber == request.InternalBatchNumber);
                if(index<0)
                    throw new InvalidOperationException($"Ligne transfert non trouvée");
                invoice.Items.RemoveAt(index);
                _transferRepository.Update(invoice);
                await _transferRepository.UnitOfWork.SaveChangesAsync();
                return default;
            }
            catch (Exception ex)
            {
                var  validations = new ValidationResult
                    {Errors = {new ValidationFailure("Transaction rolled back", ex.Message)}};
                _logger.Error(ex.Message);
                _logger.Error(ex.InnerException?.Message);
                return validations;
            }
        }
    }
}