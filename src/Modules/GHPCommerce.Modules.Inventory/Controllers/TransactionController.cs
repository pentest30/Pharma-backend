using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.Inventory.DTOs;
using GHPCommerce.Modules.Inventory.Queries.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.Inventory.Controllers
{
    [Route("api/transactions/")]
    [ApiController]
    public class TransactionController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public TransactionController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Inventory, PermissionAction.GET, "Admin", "InventoryManager")]
        [Route("/api/transactions/search")]
        public Task<SyncPagedResult<InventItemTransactionDto>> GetTransactionsSync(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetTransactionPagedQuery {SyncDataGridQuery = query});
        }
    }
}