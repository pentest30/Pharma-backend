using System;

namespace GHPCommerce.Modules.PreparationOrder.DTOs
{
    public class ConsolidationOrdersDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string OrganizationName { get; set; }
        public string CustomerName { get; set; }
        public string OrderIdentifier { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? ConsolidatedTime { get; set; }
        public string ConsolidatedByName { get; set; }
        public string EmployeeCode { get; set; }
        public string ReceivedInShippingBy { get; set; }
        public Guid ReceivedInShippingById { get; set; }
        public DateTime? ReceptionExpeditionTime { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public int TotalPackage { get; set; }
        public int TotalPackageThermolabile { get; set; }
        public bool consolidated { get; set; }
        public Guid ConsolidatedById { get; set; }
        public bool BlGenerated { get; set; }
        public string Sector { get; set; }
        public int OrderStatus { get; set; }

        public string OrderNumber => "BC-"+OrderDate?.Date.ToString("yy-MM-dd").Substring(0,2)
                                          +"/" +"0000000000".Substring(0,10-OrderIdentifier.ToString().Length)+ OrderIdentifier;
    }
}
