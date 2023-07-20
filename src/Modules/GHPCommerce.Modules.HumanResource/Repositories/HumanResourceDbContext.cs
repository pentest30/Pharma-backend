using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GHPCommerce.Modules.HumanResource.Repositories
{
    public class HumanResourceDbContext: DbContext,IUnitOfWork
    {
        private IDbContextTransaction _dbContextTransaction;
        public HumanResourceDbContext(DbContextOptions<HumanResourceDbContext> options)
            : base(options)
        {
        }

        public IDbContextTransaction GetDbContextTransaction()
        {
            return _dbContextTransaction;
        }

        public async Task SaveChangesAsync()
        {
            await base.SaveChangesAsync();
        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            _dbContextTransaction = Database.BeginTransaction(isolationLevel);
           
        }

        public void CommitTransaction()
        {
            _dbContextTransaction.Commit();
        }

        public void UserTransaction(IDbContextTransaction transaction)
        {
            Database.UseTransaction(transaction.GetDbTransaction());
        }

        protected override void OnModelCreating(ModelBuilder builder)
        
        {
            
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
        
    }
}