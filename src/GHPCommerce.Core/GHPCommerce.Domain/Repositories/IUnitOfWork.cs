using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace GHPCommerce.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// saves the changes
        /// </summary>
        /// <returns></returns>
        IDbContextTransaction GetDbContextTransaction();
        int SaveChanges();
        /// <summary>
        /// async save changes
        /// </summary>
        /// <returns></returns>
        Task SaveChangesAsync();
       /// <summary>
       /// begins a transaction
       /// </summary>
       /// <param name="isolationLevel"></param>
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        /// <summary>
        /// commits the transaction
        /// </summary>
        void CommitTransaction();

        void UserTransaction(IDbContextTransaction transaction);
    }
}
