using System.Threading.Tasks;
using GHPCommerce.Application.Catalog.RequestLogs.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/logs")]
    [ApiController]
    public class LogApiController : ApiController
    {
        private readonly ICommandBus _commandBus;
        public LogApiController(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }
        
        [HttpPost]
        [Consumes("application/json")]
        [Route("/api/logs/search")]
        public async Task<IActionResult> Get(SyncDataGridQuery query)
        {
            return Ok(await _commandBus.SendAsync(new GetLogRequestQuery {SyncDataGridQuery = query}));
        }
    }
}