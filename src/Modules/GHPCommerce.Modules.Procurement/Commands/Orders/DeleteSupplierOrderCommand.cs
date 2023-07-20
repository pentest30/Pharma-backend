using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Procurement.Commands.Orders
{
    public class DeleteSupplierOrderCommand : DeletePendingSupplierOrderCommand
    {
        
    }

    public class DeleteSupplierOrderCommandHandler : ICommandHandler<DeleteSupplierOrderCommand, ValidationResult>
    {
        private readonly IRepository<SupplierOrder, Guid> _supplierOrderRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;

        public DeleteSupplierOrderCommandHandler(IRepository<SupplierOrder, Guid> supplierOrderRepository,
            ICurrentOrganization currentOrganization,
            ICommandBus commandBus,
            ICurrentUser currentUser)
        {
            _supplierOrderRepository = supplierOrderRepository;
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _currentUser = currentUser;
        }

        public async Task<ValidationResult> Handle(DeleteSupplierOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var orgId = _currentOrganization.GetCurrentOrganizationIdAsync();
                if (orgId == default) throw new InvalidOperationException("");

                var order = await _supplierOrderRepository.Table
                    .Include(x => x.OrderItems)
                    .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken: cancellationToken);
                if (order == null )
                    throw new InvalidOperationException("Commande non trouvée");
                order.OrderStatus = ProcurmentOrderStatus.Removed;
                _supplierOrderRepository.Update(order);
                await _supplierOrderRepository.UnitOfWork.SaveChangesAsync();
                var r = await _commandBus.SendAsync(new DeletePendingSupplierOrderCommand { OrderId = request.OrderId },
                    cancellationToken);
                return r;

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