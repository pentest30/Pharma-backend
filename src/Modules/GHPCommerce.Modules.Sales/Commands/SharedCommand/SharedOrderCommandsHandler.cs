
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Orders.Commands;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.Hubs;
using GHPCommerce.Modules.Sales.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NLog;


namespace GHPCommerce.Modules.Sales.Commands.SharedCommand
{
    public class SharedOrderCommandsHandler : ICommandHandler<UpdateOrderStatusCommand, ValidationResult>
    {
        public ICommandBus CommandBus { get; }
        private readonly IOrdersRepository _ordersRepository;
        private readonly ICurrentOrganization _currentOrganization;

        private InventSumCreatedEvent _initialInventSums;
        private readonly ICache _redisCache;
        private readonly IHubContext<InventSumHub> _hubContext;
        private readonly Logger _logger;
        public SharedOrderCommandsHandler(IOrdersRepository ordersRepository,
            IMapper mapper,
            ICurrentOrganization currentOrganization,
            ICommandBus commandBus,
            ICurrentUser currentUser,
            // ILockProvider<string> lockProvider,
            ICache redisCache,
            IHubContext<InventSumHub> hubContext, Logger logger)
        {
            CommandBus = commandBus;
            _ordersRepository = ordersRepository;
            _currentOrganization = currentOrganization;
            // LockProvider<string> = lockProvider;
            _redisCache = redisCache;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<ValidationResult> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var order = await _ordersRepository.Table.Where(x => x.Id == request.Id).FirstOrDefaultAsync(cancellationToken);
            order.OrderStatus = (Entities.OrderStatus)request.OrderStatus;
            _ordersRepository.Update(order);
            try
            {
                await _ordersRepository.UnitOfWork.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _logger.Error(ex.InnerException?.Message);
            }
            return default;
        }
    }
}
