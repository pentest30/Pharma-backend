using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GHPCommerce.Modules.Inventory.Repositories
{
    public class AxDbContext: DbContext,IUnitOfWork
    {
       
        public AxDbContext(DbContextOptions<AxDbContext> options)
            : base(options)
        {
            
        }
        public IDbContextTransaction GetDbContextTransaction()
        {
            throw new System.NotImplementedException();
        }

        public Task SaveChangesAsync()
        {
            throw new System.NotImplementedException();
        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            throw new System.NotImplementedException();
        }

        public void CommitTransaction()
        {
            throw new System.NotImplementedException();
        }

        public void UserTransaction(IDbContextTransaction transaction)
        {
            throw new System.NotImplementedException();
        }
        protected override void OnModelCreating(ModelBuilder builder)
        
        {
            
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}