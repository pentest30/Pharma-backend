using GHPCommerce.Core.Shared.Contracts.Invoices.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using System;

namespace GHPCommerce.Core.Shared.Contracts.Invoices.Queries
{
    public class GetInvoiceByCustomersQuery : ICommand<InvoiceDtoV3>
    {
        public DateTime Date { get; set; } 
        public Guid? CustomerId { get; set; }
    }
}
