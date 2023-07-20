﻿using System;
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
    public class BackToSaveTransferLogCommand : SaveTransferLogCommand
    {
    }
    public class BackToSaveTransferLogCommandHandler : ICommandHandler<BackToSaveTransferLogCommand, ValidationResult>
    {
        private readonly IRepository<TransferLog, Guid> _transferRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly Logger _logger;

        public BackToSaveTransferLogCommandHandler(IRepository<TransferLog, Guid> transferRepository,  
            ICurrentOrganization currentOrganization,
            IMapper mapper,
            Logger logger)
        {
            _transferRepository = transferRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<ValidationResult> Handle(BackToSaveTransferLogCommand request, CancellationToken cancellationToken)
        {
            var validations = default(ValidationResult);
            try
            {
                var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (orgId == default) throw new InvalidOperationException("Resources not allowed");
                var transferLog = await _transferRepository.Table
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
                if (transferLog == null || transferLog.Status != TransferLogStatus.Valid)
                    throw new NotFoundException($"Transfer log with  {request.Id} was not found");
                transferLog.Status = TransferLogStatus.Saved;
                _transferRepository.Update(transferLog);
                await _transferRepository.UnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                validations = new ValidationResult { Errors = { new ValidationFailure("Transaction rolled back", ex.Message) } };
                _logger.Error(ex.Message);
                _logger.Error(ex.InnerException?.Message);
            }
            return validations;
        }
    }
}