using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Modules.Quota.DTOs;

namespace GHPCommerce.Modules.Quota.Queries.ReceivedQuota
{
    public class GetPagedReceivedQuotasQuery:ICommand<SyncPagedResult<ReceivedQuotaDto>>
    {
        public SyncDataGridQuery DataGridQuery { get; set; }
        
    }
}