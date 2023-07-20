using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.PreparationOrder.Entities;
using GHPCommerce.Modules.PreparationOrder.Repositories;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GHPCommerce.Modules.PreparationOrder.Commands
{
    public class ControlPreparationOrderCommand: ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
    }

    public class ControlPreparationOrderCommandHandler : ICommandHandler<ControlPreparationOrderCommand, ValidationResult>
    {
        private readonly ICommandBus _commandBus;
        private readonly IPreparationOrderRepository _preparationOrderRepository;
        private readonly Logger _logger;
        private readonly ICurrentOrganization _currentOrganization;

        public ControlPreparationOrderCommandHandler(ICommandBus commandBus,
            IPreparationOrderRepository preparationOrderRepository,
            Logger logger,
            ICurrentOrganization currentOrganization)
        {
            _commandBus = commandBus;
            _preparationOrderRepository = preparationOrderRepository;
            _logger = logger;
            _currentOrganization = currentOrganization;
        }

        public async Task<ValidationResult> Handle(ControlPreparationOrderCommand request, CancellationToken cancellationToken)
        {
            var op = await _preparationOrderRepository.Table
                .AsTracking()
                .Include(c => c.PreparationOrderItems)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            if (op.PreparationOrderItems.All(c => c.IsControlled) )
            {
                if(op.PreparationOrderStatus == 0 || op.PreparationOrderStatus == PreparationOrderStatus.Prepared) op.PreparationOrderStatus = PreparationOrderStatus.Controlled;
                await _preparationOrderRepository.UnitOfWork.SaveChangesAsync();
            }
            else
            {
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("OP Non Controllé", "Il existe des lignes non controllés !")
                    }
                };
            }

            return default;
        }
    }
}