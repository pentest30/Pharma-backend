using System.Linq;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Repositories
{
    public interface IRepository<TEntity, TKey> where TEntity : AggregateRoot<TKey>
    {
        IUnitOfWork UnitOfWork { get; }

        IQueryable<TEntity> Table { get; }

        void AddOrUpdate(TEntity entity);
        void Add(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        void UpdateEntity(TEntity entity);
    }
}