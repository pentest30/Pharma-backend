using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.PreparationOrder.Entities
{
    public class PreparationOrder : AggregateRoot<Guid>,IEntitySequenceNumber
    {
        public PreparationOrder()
        {
            PreparationOrderItems = new List<PreparationOrderItem>();
            PreparationOrderExecuters = new List<PreparationOrderExecuter>();
            PreparationOrderVerifiers = new List<PreparationOrderVerifier>();
        }
        public Guid CustomerId { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string CustomerName { get; set; }
        //public string IdentifierNumber { get; set; }
        /// <summary>
        /// gets or sets order number sequence
        /// </summary>

        public string IdentifierNumber { get; set; }
        public int PreparationOrderNumberSequence { get; set; }
        public string BarCode { get; set; }
        public byte[] BarCodeImage { get; set; }

        public int TotalPackage { get; set; }
        public int TotalPackageThermolabile { get; set; }
        public int TotalZoneCount { get; set; }
        public int ZoneDoneCount { get; set; }
        public bool Printed { get; set; }
        public int PrintCount { get; set; }
        public Guid? PrintedBy { get; set; }
        public string PrintedByName { get; set; }
        public DateTime? PrintedTime { get; set; }
        public Guid OrderId { get; set; }
        public DateTime? OrderDate { get; set; }

        public int ZoneGroupOrder { get; set; }
        public string OrderIdentifier { get; set; }

        public Guid ZoneGroupId { get; set; }
        public string SectorName { get; set; }
        public string SectorCode { get; set; }
        public string ZoneGroupName { get; set; }
        public Guid? ConsolidatedById { get; set; }
        public DateTime? ConsolidatedTime { get; set; }
        public string ConsolidatedByName { get; set; }
        public string EmployeeCode { get; set; }
        public bool ToBeRespected { get; set; }

        public PreparationOrderStatus PreparationOrderStatus { get; set; }

        public List<PreparationOrderExecuter> PreparationOrderExecuters { get; set; }
        public List<PreparationOrderVerifier> PreparationOrderVerifiers { get; set; }
        public List<PreparationOrderItem> PreparationOrderItems { get; set; }
        public int SequenceNumber { get; set; }
        public string CodeAx { get; set; }


    }
}
