using System;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GHPCommerce.Modules.PreparationOrder.Repositories
{
    public class PreparationOrderDbContext : DbContext, IUnitOfWork
    {
        public PreparationOrderDbContext(DbContextOptions<PreparationOrderDbContext> options)
            : base(options)
        {

        }
        private IDbContextTransaction _dbContextTransaction;
        public IDbContextTransaction GetDbContextTransaction()
        {
            throw new NotImplementedException();
        }

        public Task SaveChangesAsync()
        {
            var ret=base.SaveChangesAsync();
            return ret;
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            _dbContextTransaction = Database.BeginTransaction(isolationLevel);
        }

        public void CommitTransaction()
        {
            throw new NotImplementedException();
        }

        public void UserTransaction(IDbContextTransaction transaction)
        {
            throw new NotImplementedException();
        }
    }
}
