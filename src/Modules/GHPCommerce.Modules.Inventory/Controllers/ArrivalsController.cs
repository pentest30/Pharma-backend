using System.Threading.Tasks;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.Inventory.Entities;
using GHPCommerce.Modules.Inventory.Queries.Arrival;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace GHPCommerce.Modules.Inventory.Controllers
{
    [Route("api/arrivals/")]
    [ApiController]
    public class ArrivalsController:ApiController
    {
        private readonly ICommandBus _commandBus;
        public ArrivalsController(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
       // [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.POST, "SalesPerson", "InventoryManager", "Supervisor")]
        [Route("/api/arrivals/search")]
        public Task<SyncPagedResult<Arrivals>> GetInventSync(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetArrivalsPagedQuery { SyncDataGridQuery = query });
        }
    }
}