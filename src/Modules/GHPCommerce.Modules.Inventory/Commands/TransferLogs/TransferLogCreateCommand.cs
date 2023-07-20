using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Commands.TransferLogs
{
    public class TransferLogCreateCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid ZoneSourceId { get; set; }
        public string ZoneSourceName { get; set; }
        public Guid ZoneDestId { get; set; }
        public string ZoneDestName { get; set; }
        public TransferLogStatus Status { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string InternalBatchNumber { get; set; }
        public Guid InventId { get; set; }
        public double Quantity { get; set; }
        public DateTime ExpiryDate { get; set; }
        public Guid StockStateId { get; set; }
        public string StockStateName { get; set; }
        public Guid StockStateSourceId { get; set; }
        public string StockStateSourceName { get; set; }

    }
    public class TransferLogCreateCommandHandler : ICommandHandler<TransferLogCreateCommand, ValidationResult>
    {
        private readonly IRepository<TransferLog, Guid> _transferRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ISequenceNumberService<TransferLog, Guid> _sequenceNumberService;

        public TransferLogCreateCommandHandler(IRepository<TransferLog, Guid> transferRepository, 
            ICurrentOrganization currentOrganization,
            IMapper mapper, 
            ISequenceNumberService<TransferLog, Guid> sequenceNumberService)
        {
            _transferRepository = transferRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _sequenceNumberService = sequenceNumberService;
        }

        public async Task<ValidationResult> Handle(TransferLogCreateCommand request, CancellationToken cancellationToken)
        {
            var validation = default(ValidationResult);
            try
            {
                var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (orgId == default) throw new InvalidOperationException("Resources not allowed");
                bool newTransfer;
                var transferLog = await _transferRepository.Table
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
                if (transferLog == null)
                {
                    transferLog = _mapper.Map<TransferLog>(request);
                    transferLog.OrganizationId = orgId.Value;
                    transferLog.Status = TransferLogStatus.Created;
                    var keysq = nameof(TransferLog) + orgId;
                    await LockProvider<string>.WaitAsync(keysq, cancellationToken);
                    var sq = await _sequenceNumberService.GenerateSequenceNumberAsync(DateTime.Now, orgId.Value);
                    transferLog.SequenceNumber = sq;
                    LockProvider<string>.Release(keysq);
                    newTransfer = true;
                }
                else newTransfer = false;
                var item = _mapper.Map<TransferLogItem>(request);
                item.Id = Guid.Empty;
                if (!transferLog.Items
                        .Any(x => x.ProductId == request.ProductId
                                  && x.InternalBatchNumber == request.InternalBatchNumber)) transferLog.Items.Add(item);
                else throw new InvalidOperationException("Journal de transfert avec le code porduit et le n° de lot existe déjà dans la liste ");
                if (newTransfer) _transferRepository.Add(transferLog);
                else _transferRepository.Update(transferLog);
                await _transferRepository.UnitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                validation = new ValidationResult
                    { Errors = { new ValidationFailure("Validation errors ", e.Message) } };

            }
           
            return validation;

        }
    }
}