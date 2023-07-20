using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Modules.Quota.DTOs;

namespace GHPCommerce.Modules.Quota.Queries.QuotaRequest
{
    public class GetPagedRequestedQuotasQuery:ICommand<SyncPagedResult<QuotaRequestDto>>
    {
    public SyncDataGridQuery DataGridQuery { get; set; }
    }
}