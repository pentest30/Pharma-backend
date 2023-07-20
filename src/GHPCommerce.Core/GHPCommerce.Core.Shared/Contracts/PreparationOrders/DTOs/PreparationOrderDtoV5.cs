using System;

namespace GHPCommerce.Core.Shared.Contracts.PreparationOrders.DTOs
{
    public class PreparationOrderDtoV5 
    {
        public string ConsolidatedByName { get; set; }
        public string ZoneGroupName { get; set; }
        public DateTime? ConsolidatedTime { get; set; }
        public PreparationOrderStatus PreparationOrderStatus { get; set; }
        public string SectorName { get; set; }
        public string SectorCode { get; set; }
        public Guid OrderId { get; set; }
        public int OrderNumberSequence { get; set; }
        public int TotalPackageThermolabile { get; set; }
        public int TotalPackage { get; set; }
    }
    public enum PreparationOrderStatus : uint
    {
        Prepared = 10,
        Controlled = 20,
        Consolidated = 30,
        Valid = 40,
        ReadyToBeShipped = 50,
        CancelledOrder = 500
    }
}
