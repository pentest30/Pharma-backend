using System.Linq;
using GHPCommerce.CrossCuttingConcerns.OS;
using GHPCommerce.Domain.Domain.Common;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.HumanResource.Repositories
{
    public class Repository<T, TKey> : IRepository<T, TKey> where T : AggregateRoot<TKey>
    {
        protected readonly HumanResourceDbContext DbContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICurrentUser _currentUser;

        protected DbSet<T> DbSet => DbContext.Set<T>();

        public IUnitOfWork UnitOfWork => DbContext;

        public Repository(HumanResourceDbContext dbContext, IDateTimeProvider dateTimeProvider,ICurrentUser currentUser)
        {
            DbContext = dbContext;
            _dateTimeProvider = dateTimeProvider;
            _currentUser = currentUser;
        }

        public void AddOrUpdate(T entity)
        {
            if (entity.Id.Equals(default(TKey)))
            {
                entity.CreatedDateTime = _dateTimeProvider.OffsetNow;
                entity.CreatedByUserId = _currentUser.UserId;
                DbSet.Add(entity);
            }
            else
            {
                entity.UpdatedByUserId = _currentUser.UserId;
                entity.UpdatedDateTime = _dateTimeProvider.OffsetNow;
            }
        }

        public void Add(T entity)
        {
            entity.CreatedDateTime = _dateTimeProvider.OffsetNow;
            if(_currentUser!=null && _currentUser.UserId!=default) entity.CreatedByUserId = _currentUser.UserId;
            DbSet.Add(entity);
        }

        public void Update(T entity)
        {
            //DbContext.Entry(entity).State = EntityState.Modified;
            entity.UpdatedDateTime = _dateTimeProvider.OffsetNow;
            if(_currentUser!=null) entity.UpdatedByUserId = _currentUser.UserId;
        }

        public void Delete(T entity)
        {
            DbSet.Remove(entity);
        }
        
        public IQueryable<T> Table => DbContext.Set<T>();

        public void UpdateEntity(T entity)
        {
            DbContext.Update(entity);
        }
    }
}