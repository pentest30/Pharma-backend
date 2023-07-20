using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Sales.Commands.DynamicsAx
{
    public class SyncDriverNameCommand : ICommand<ValidationResult>
    {
        public string CodeAx { get; set; }
        public string DriverName { get; set; }
    }
    public class SyncDriverNameCommandHandler : ICommandHandler<SyncDriverNameCommand, ValidationResult>
    {
        private readonly IRepository<Order, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;
  
        private readonly ICommandBus _commandBus;

        public SyncDriverNameCommandHandler(IRepository<Order, Guid> repository, 
            ICurrentOrganization currentOrganization, 
          
            ICommandBus commandBus)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
        }
        public async Task<ValidationResult> Handle(SyncDriverNameCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (!orgId.HasValue)
                    throw new InvalidOperationException("Resources not allowed");
                if (string.IsNullOrEmpty(request.CodeAx))
                {
                    throw new InvalidOperationException("Order number is empty");
                }
                var order = await _repository.Table
                    .Include(x => x.OrderItems)
                    .FirstOrDefaultAsync(x => x.CodeAx == request.CodeAx
                                              && x.SupplierId == orgId.Value, cancellationToken: cancellationToken);
                if (order == default)
                    throw new NotFoundException($"Order with number {request.CodeAx} was not found");

                order.DriverName = request.DriverName;
                _repository.Update(order);
                await _repository.UnitOfWork.SaveChangesAsync();
                return default;
            }
            catch (Exception ex)
            {
                var validations = new ValidationResult
                    { Errors = { new ValidationFailure("Transaction rolled back", ex.Message) } };
                
                return validations;
            }

        }
    }
}