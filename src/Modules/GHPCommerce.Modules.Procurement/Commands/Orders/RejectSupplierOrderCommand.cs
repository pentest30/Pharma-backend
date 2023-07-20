using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GHPCommerce.Modules.Procurement.Commands.Orders
{
    public class RejectSupplierOrderCommand : SaveSupplierOrderCommand
    {
        
    }
    public class RejectSupplierOrderCommandHandler:ICommandHandler<RejectSupplierOrderCommand, ValidationResult>
    {
        private readonly IRepository<SupplierOrder, Guid> _supplierOrderRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly ICache _redisCache;
        private readonly Logger _logger;
        private const string CACHE_KEY = "_supplier-order";

        public RejectSupplierOrderCommandHandler(IRepository<SupplierOrder, Guid> supplierOrderRepository,
            IMapper mapper, 
            ICurrentOrganization currentOrganization, 
            ICommandBus commandBus, 
            ICurrentUser currentUser, 
            ICache redisCache, 
            Logger logger)
        {
            _supplierOrderRepository = supplierOrderRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _currentUser = currentUser;
            _redisCache = redisCache;
            _logger = logger;
        }
        public async Task<ValidationResult> Handle(RejectSupplierOrderCommand request, CancellationToken cancellationToken)
        {
           try
            {
                var orgId = _currentOrganization.GetCurrentOrganizationIdAsync();
                if (orgId == default) throw new InvalidOperationException("");
               
                var order = await _supplierOrderRepository.Table.Include(x => x.OrderItems)
                    .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken: cancellationToken);
                if (order == null )
                    throw new InvalidOperationException("Commande non trouvée");
           
                order.OrderStatus = ProcurmentOrderStatus.Rejected;
                _supplierOrderRepository.Update(order);
                await _supplierOrderRepository.UnitOfWork.SaveChangesAsync();
                var r = await _commandBus.SendAsync(new DeletePendingSupplierOrderCommand { OrderId = request.OrderId },
                    cancellationToken);
                return r;
            }
            catch (Exception ex)
            {
                var validations = new ValidationResult
                    {Errors = {new ValidationFailure("Transaction rolled back", ex.Message)}};
                return validations;
            }

            return default;
        }
       
    }
}