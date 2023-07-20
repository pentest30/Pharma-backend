using System.Diagnostics;
using System.Threading;
using System.Transactions;

namespace GHPCommerce.Infra.OS.Threads
{
    public class WorkerThread
    {
        public void DoWork(DependentTransaction dependentTransaction)
        {
            Thread thread = new Thread(ThreadMethod);
            thread.Start(dependentTransaction);
        }

        public void ThreadMethod(object transaction)
        {
            DependentTransaction dependentTransaction = transaction as DependentTransaction;
            Debug.Assert(dependentTransaction != null);
            try
            {
                using (TransactionScope ts = new TransactionScope(dependentTransaction))
                {
                    /* Perform transactional work here */
                    ts.Complete();
                }
            }
            finally
            {
                dependentTransaction.Complete();
                dependentTransaction.Dispose();
            }
        }

    }
}
