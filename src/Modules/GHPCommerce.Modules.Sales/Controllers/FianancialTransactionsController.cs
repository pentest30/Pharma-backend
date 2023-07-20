using System.Threading.Tasks;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.Sales.DTOs.FinancialTransactions;
using GHPCommerce.Modules.Sales.Queries.FinancialTransactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.Sales.Controllers
{
    [Route("api/financial-transaction")]
    [ApiController]
    public class FianancialTransactionsController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;

        public FianancialTransactionsController(ICommandBus commandBus, ICurrentUser currentUser)
        {
            _commandBus = commandBus;
            _currentUser = currentUser;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [ResourceAuthorization(PermissionItem.Sales, PermissionAction.Create, "Admin", "SalesManager", "SalesPerson")]
        [Route("/api/financial-transaction/search")]
        public Task<SyncPagedResult<FinancialTransactionDto>> GetFinancialTransactions(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetPagedFinancialTransactionQuery { SyncDataGridQuery = query });
        }
    }
}