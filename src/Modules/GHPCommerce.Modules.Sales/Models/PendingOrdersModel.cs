using System;

namespace GHPCommerce.Modules.Sales.Models
{
    public class PendingOrdersModel
    {
        public Guid Id { get; set; }
        public Guid SalesPersonId { get; set; }
        public Guid CustomerId { get; set; }
    }
}