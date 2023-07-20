using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Modules.Quota.DTOs;

namespace GHPCommerce.Modules.Quota.Queries
{
    public class GetPagedQuotasQuery : ICommand<SyncPagedResult<QuotaDto>>
    {
        public SyncDataGridQuery DataGridQuery { get; set; }
    }
}