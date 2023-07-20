using System;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Batches.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.Inventory.DTOs.Batches;
using GHPCommerce.Modules.Inventory.Queries.Batches;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.Inventory.Controllers
{
    [Route("api/batches/")]
    [ApiController]
    public class BatchController :ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public BatchController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.GET, "Admin", "InventoryManager")]
        [Route("/api/batches/search")]
        public Task<SyncPagedResult<BatchDto>> GetInventSync(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetBatchPagedQuery { SyncDataGridQuery = query });
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.GET, "Admin", "InventoryManager", "Buyer", "BuyerGroup")]
        [Route(template: "/api/batches/{supplierId:guid}/internal-batch")]
        public Task<object> GetInventSync([FromRoute]Guid supplierId, GetInternalBatchNumberQuery query)
        {
            return _commandBus.SendAsync(query);
        }
    }
}