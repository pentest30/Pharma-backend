using System.Collections.Generic;

namespace GHPCommerce.Domain.Domain.Queries
{
    public class SyncPagedResult <T> 
    {
        public IEnumerable<T> Result { get; set; }

        public int Count { get; set; }
    }

    public class CustomSyncPagedResult<T> : SyncPagedResult<T>
    {
        public List<object> Objects { get; set; }
    }
}
