using System;

namespace GHPCommerce.Modules.LegalActions.Entities
{
    public class SeizureRequest
    {
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Court { get; set; }
        public DateTime DepositDate { get; set; }
        
    }
}