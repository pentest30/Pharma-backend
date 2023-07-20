using System;
using System.Linq;
using System.Threading.Tasks;
using GHPCommerce.Domain.Domain.Common;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Core.Shared.Services
{
    public class SequenceNumberService<T,TKey> : ISequenceNumberService<T, TKey> where T: AggregateRoot<TKey>,  IEntitySequenceNumber
    {
        private readonly IRepository<T, TKey> _repository;
        public SequenceNumberService(IRepository<T, TKey> repository)
        {
            _repository = repository;
        }

        public async Task<int> GenerateSequenceNumberAsync(DateTime date, Guid orgId)
        {
            var query = await _repository.Table
                .OrderByDescending(x => x.SequenceNumber)
                .Where(x => x.CreatedDateTime.Year == date.Year && x.OrganizationId == orgId)
                .Select(x => x.SequenceNumber)
                .FirstOrDefaultAsync();
            return query + 1;
        }

        public async Task<int> GenerateSequenceNumberAsync(Guid orgId) 
        {
            var query =await _repository.Table
                .OrderByDescending(x => x.SequenceNumber)
                .Where(x =>  x.OrganizationId == orgId )
                .Select(x=>x.SequenceNumber)
                .FirstOrDefaultAsync();
            return query + 1;
        }
    }
}