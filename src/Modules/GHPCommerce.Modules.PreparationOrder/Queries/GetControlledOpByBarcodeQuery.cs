using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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
    public class GetControlledOpByBarcodeQuery : ICommand<PreparationOrdersControlValidationDto>
    {
        public string BarCode { get; set; }
    }
    public class PreparationOrdersControlValidationDto
    {
        public Dictionary<string,string> ErrorMessages { get; set; }
        public PreparationOrdersDto PreparationOrder { get; set; }
    }
     public class GetControlledOpByBarcodeQueryHandler :
        ICommandHandler<GetControlledOpByBarcodeQuery, PreparationOrdersControlValidationDto>


    {
        private readonly IPreparationOrderRepository _preparationOrdersRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICurrentUser _currentUser;
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public GetControlledOpByBarcodeQueryHandler(
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


        public async Task<PreparationOrdersControlValidationDto> Handle(GetControlledOpByBarcodeQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
            {
                var errors = new Dictionary<string, string>();
                errors.Add("Error", "Organisation null");
                return new PreparationOrdersControlValidationDto { ErrorMessages = errors, PreparationOrder = default };
            }
            if (request.BarCode.Length != 13)
            {
                var errors = new Dictionary<string, string>();
                errors.Add("Error", "Taille code à barre erroné");
                return new PreparationOrdersControlValidationDto { ErrorMessages = errors, PreparationOrder = default };
            }


            var result = await _preparationOrdersRepository.Table
                .Include(c => c.PreparationOrderItems)
                .Where(x => x.OrganizationId == org && x.BarCode == request.BarCode )
                .FirstOrDefaultAsync();
            
           
            if (result == null)
            {
                var errors = new Dictionary<string, string>();
                errors.Add("Error", "Ordre de  préparation non trouvé");
                return new PreparationOrdersControlValidationDto { ErrorMessages = errors, PreparationOrder = default };
            } 
            else
            {
                var errors = new Dictionary<string, string>();
                switch (result.PreparationOrderStatus )
                {  
                    case 0:
                      
                        errors.Add("Error", "Ordre de  préparation non préparé");
                        return new PreparationOrdersControlValidationDto { ErrorMessages = errors, PreparationOrder = default };
                    case PreparationOrderStatus.Prepared:
                      
                        errors.Add("Error", "Ordre de  préparation en zone de préparation");
                        return new PreparationOrdersControlValidationDto { ErrorMessages = errors, PreparationOrder = default };
                    case PreparationOrderStatus.CancelledOrder:
                      
                        errors.Add("Error", "Ordre de  préparation Annulé");
                        return new PreparationOrdersControlValidationDto { ErrorMessages = errors, PreparationOrder = default };

                    case PreparationOrderStatus.ReadyToBeShipped:
                      
                        errors.Add("Error", "Ordre de  préparation en zone d'expédition");
                        return new PreparationOrdersControlValidationDto {ErrorMessages = errors, PreparationOrder = default};
                }
            };
            var data = _mapper.Map<PreparationOrdersDto>(result);
            if (result.PreparationOrderItems.All(c => c.Status == BlStatus.Deleted))
            {
                data.IsAllLinesDeleted = true;
            }
            return new PreparationOrdersControlValidationDto { ErrorMessages = new Dictionary<string, string>(), PreparationOrder = data};

        }
    }

    
}