using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Commands
{
    public class UpdateDebtCommand : ICommand
    {
      
        public decimal CurrentDebt { get; set; }
        public Guid Id { get; set; }
    }
}
