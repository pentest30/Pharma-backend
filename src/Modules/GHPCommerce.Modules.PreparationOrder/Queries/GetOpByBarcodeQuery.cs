using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Contracts.Orders.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.PreparationOrder.DTOs;
using GHPCommerce.Modules.PreparationOrder.Entities;
using GHPCommerce.Modules.PreparationOrder.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.PreparationOrder.Queries
{
    public class GetOpByBarcodeQuery: ICommand<PreparationOrdersDtoValidation>
    {
        public string BarCode { get; set; }
            
    }

    public class PreparationOrdersDtoValidation
    {
        public Dictionary<string,string> ErrorMessages { get; set; }
        public PreparationOrdersDtoV2 PreparationOrder { get; set; }
    }

    class GetOpByBarcodeQueryHandler :
        ICommandHandler<GetOpByBarcodeQuery, PreparationOrdersDtoValidation>


    {
        private readonly IPreparationOrderRepository _preparationOrdersRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICurrentUser _currentUser;
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public GetOpByBarcodeQueryHandler(
            IPreparationOrderRepository preparationOrdersRepository,
            ICurrentOrganization currentOrganization,
            ICurrentUser currentUser,
            ICommandBus commandBus,
            IMapper mapper)
        {
            _preparationOrdersRepository = preparationOrdersRepository;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _commandBus = commandBus;
            _mapper = mapper;
        }

        public async Task<PreparationOrdersDtoValidation> Handle(GetOpByBarcodeQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return default;
            var pickingZone = request.BarCode.Substring(13);
            var result = await _preparationOrdersRepository.Table
                .AsNoTracking()
                .Include(c =>c.PreparationOrderItems)
                .Include(c =>c.PreparationOrderVerifiers)
                .Include(c =>c.PreparationOrderExecuters)
                .Where(x =>
                    x.OrganizationId == org &&
                    x.Printed && x.BarCode == request.BarCode.Substring(0, 13))
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (result == null)
            {
                var errors = new Dictionary<string, string>();
                errors.Add("Error", "Ordre de  préparation non trouvé");
                return new PreparationOrdersDtoValidation { ErrorMessages = errors, PreparationOrder = default };
            }
            else
            {
                var errors = new Dictionary<string, string>();
                switch (result.PreparationOrderStatus )
                {  
                    case PreparationOrderStatus.Controlled:
                      
                        errors.Add("Error", "Ordre de  préparation controllé");
                        return new PreparationOrdersDtoValidation { ErrorMessages = errors, PreparationOrder = default };
                    case PreparationOrderStatus.Consolidated:
                      
                        errors.Add("Error", "Ordre de  préparation consolidé");
                        return new PreparationOrdersDtoValidation { ErrorMessages = errors, PreparationOrder = default };
                    case PreparationOrderStatus.ReadyToBeShipped:
                      
                        errors.Add("Error", "Ordre de  préparation en zone d'expédition");
                        return new PreparationOrdersDtoValidation {ErrorMessages = errors, PreparationOrder = default};
                }
            };
            
            // check if the order is not cancelled
            var order = await _commandBus.SendAsync(new GetOrderByIdQueryV2 { Id = result.OrderId }, cancellationToken);
            if (order.OrderStatus == 70)
            {
                var errors = new Dictionary<string, string>();
                errors.Add("Error", "La commande a été annulée par le service commercial ");
                return new PreparationOrdersDtoValidation { ErrorMessages = errors, PreparationOrder = default };
            }

            var items =   result.PreparationOrderItems.Where(c => !c.IsControlled && c.PickingZoneOrder == Int32.Parse(pickingZone)).AsQueryable().ToList();
            if (items.Count == 0)
            {
                var errors = new Dictionary<string, string>();
                errors.Add("Error", "Toutes les lignes ont été cotrôlées");
                return new PreparationOrdersDtoValidation { ErrorMessages = errors, PreparationOrder = default };
            };
            result.PreparationOrderItems = items.
                OrderBy(c => String.IsNullOrEmpty(c.DefaultLocation)).
                ThenBy(c => c.ProductName)
                .ToList();
            return new PreparationOrdersDtoValidation { ErrorMessages = new Dictionary<string, string>(), PreparationOrder = _mapper.Map<PreparationOrdersDtoV2>(result) };
           
        }
    }
}