using System.Linq;
using GHPCommerce.CrossCuttingConcerns.OS;
using GHPCommerce.Domain.Domain.Common;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Persistence.Repositories
{
    public class Repository<T, TKey> : IRepository<T, TKey> where T : AggregateRoot<TKey>
    {
        protected readonly ApplicationDbContext DbContext;
        private readonly IDateTimeProvider _dateTimeProvider;

        protected DbSet<T> DbSet => DbContext.Set<T>();

        public IUnitOfWork UnitOfWork => DbContext;

        public Repository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
        {
            DbContext = dbContext;
            _dateTimeProvider = dateTimeProvider;
        }

        public void AddOrUpdate(T entity)
        {
            if (entity.Id.Equals(default(TKey)))
            {
                entity.CreatedDateTime = _dateTimeProvider.OffsetNow;
                DbSet.Add(entity);
            }
            else
            {
                entity.UpdatedDateTime = _dateTimeProvider.OffsetNow;
            }
        }

        public void Add(T entity)
        {
            entity.CreatedDateTime = _dateTimeProvider.OffsetNow;
            DbSet.Add(entity);
        }

        public void Update(T entity)
        {
            //DbContext.Entry(entity).State = EntityState.Modified;
            entity.UpdatedDateTime = _dateTimeProvider.OffsetNow;
        }
        public void UpdateEntity(T entity)
        {
            DbContext.Update(entity);
        }
        public void Delete(T entity)
        {
            DbSet.Remove(entity);
        }

        public IQueryable<T> Table => DbContext.Set<T>();
        
    }
}
