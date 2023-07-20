using System;
using System.Threading.Tasks;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Services
{
    public interface ISequenceNumberService< in T, TKey> where T : AggregateRoot<TKey>, IEntitySequenceNumber
    {
        Task<int> GenerateSequenceNumberAsync( DateTime date, Guid orgId) ;
        Task<int> GenerateSequenceNumberAsync(Guid orgId);
    }
}