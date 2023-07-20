using GHPCommerce.Core.Shared.Contracts.Invoices.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using System;

namespace GHPCommerce.Core.Shared.Contracts.Invoices.Queries
{
    public class GetInvoiceByCustomerIdQuery : ICommand<InvoiceDtoV2>
    {
        public DateTime Date { get; set; }
        public Guid OrganizationId { get; set; }
    }
}
