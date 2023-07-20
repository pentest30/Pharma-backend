using GHPCommerce.Modules.PreparationOrder.Entities;
using System;

namespace GHPCommerce.Modules.PreparationOrder.DTOs
{
    public class PreparationOrdersDto
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid OrderId { get; set; }
        public string OrganizationName { get; set; }
        public string BarCode { get; set; }
        public int TotalPackage { get; set; }
        public int TotalPackageThermolabile { get; set; }
        public int TotalZoneCount { get; set; }
        public int ZoneDoneCount { get; set; }
        public int ZoneGroupOrder { get; set; }
        public string OrderIdentifier { get; set; }
        public DateTime? OrderDate { get; set; }

        public bool Printed { get; set; }
        public string PrintedByName { get; set; }
        public DateTime? PrintedTime { get; set; }
        public string ZoneGroupName { get; set; }
        public Guid ZoneGroupId { get; set; }

        public DateTime? ConsolidatedTime { get; set; }
        public string ConsolidatedByName { get; set; }
        public string ReceivedByCode { get; set; }
        public string EmployeeCode { get; set; }

        public string SectorName { get; set; }
        public string IdentifierNumber { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public PreparationOrderStatus PreparationOrderStatus { get; set; }
        public bool IsConsolidated { get; set; }

        public string OrderNumber => "BC-"+OrderDate?.Date.ToString("yy-MM-dd").Substring(0,2)
                                          +"/" +"0000000000".Substring(0,10-OrderIdentifier.ToString().Length)+ OrderIdentifier;

        public bool IsAllLinesDeleted { get; set; }
    }
}
