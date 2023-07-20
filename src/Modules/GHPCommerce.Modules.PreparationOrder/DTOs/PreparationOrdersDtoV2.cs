using GHPCommerce.Modules.PreparationOrder.Entities;
using System;
using System.Collections.Generic;

namespace GHPCommerce.Modules.PreparationOrder.DTOs
{
    public class PreparationOrdersDtoV2
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string CustomerName { get; set; }
        public int TotalPackage { get; set; }
        public int TotalPackageThermolabile { get; set; }
        public DateTime? OrderDate { get; set; }

        public string OrderIdentifier { get; set; }
        public string SectorName { get; set; }
        public int TotalZoneCount { get; set; }
        public int ZoneDoneCount { get; set; }
        public Guid OrderId { get; set; }
        public Guid ZoneGroupId { get; set; }
        public string ZoneGroupName { get; set; }
        public int? PackingQuantity { get; set; }
        public int Packing { get; set; }
        public Guid? ConsolidatedById { get; set; }
        public string ConsolidatedByName { get; set; }
        public DateTime? ConsolidatedTime { get; set; }
        public string EmployeeCode { get; set; }
        public string ReceivedByCode { get; set; }
        public DateTimeOffset? CreatedDateTime { get; set; }
        public int SequenceNumber { get; set; }
        public int QuantityToControl { get; set; }
        public string OrderNumber => "BC-"+OrderDate?.Date.ToString("yy-MM-dd").Substring(0,2)
                                          +"/" +"0000000000".Substring(0,10-OrderIdentifier.ToString().Length)+ OrderIdentifier;
        public List<PreparationOrderExecuter> PreparationOrderExecuters { get; set; }
        public List<PreparationOrderVerifier> PreparationOrderVerifiers { get; set; }
        public List<PreparationOrderItemDto> PreparationOrderItems { get; set; }
        public int CountVerifiers => PreparationOrderVerifiers != null ? PreparationOrderVerifiers.Count : 0;
        public int CountExecuters => PreparationOrderExecuters != null ? PreparationOrderExecuters.Count : 0;
    }
}
