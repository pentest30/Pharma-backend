using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Modules.PreparationOrder.Entities;
using System;
using System.Text;

namespace GHPCommerce.Modules.PreparationOrder.DTOs
{
    public class PreparationOrderDtoV4
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid OrderId { get; set; }
        public string OrganizationName { get; set; }
        public string BarCode { get; set; }
        public Guid PickingZoneId { get; set; }
        public string PickingZoneName { get; set; }
        public int PickingZoneOrder { get; set; }

        public string OrderIdentifier { get; set; }
        public DateTime? OrderDate { get; set; }
        public string ZoneGroupName { get; set; }
        public string VerifiedByName { get; set; }
        public DateTime VerifiedTime { get; set; }
        public string OrderNumber => "BC-" + OrderDate?.Date.ToString("yy-MM-dd").Substring(0, 2)
                                          + "/" + "0000000000".Substring(0, 10 - OrderIdentifier.ToString().Length) + OrderIdentifier;
        public PreparationOrderStatus PreparationOrderStatus { get; set; }
        public int CountNotControlled { get; set; }
        public int CountVerifiers { get; set; }
        public int CountExecuters { get; set; }
        public int TotalPackage { get; set; }
        public int TotalPackageThermolabile { get; set; }
        public string TotalPackageAsString { 
            get 
            {

                var sb= (int)PreparationOrderStatus>10?
                    (TotalPackage >0?
                    (TotalPackageThermolabile > 0 ? new StringBuilder($"{TotalPackage}").Append(" (A) + ").Append($"{TotalPackageThermolabile} (F)")
                    : new StringBuilder($"{TotalPackage}"))
                    : (TotalPackageThermolabile == 0 ? new StringBuilder("") : new StringBuilder($"{(TotalPackageThermolabile)}").Append(" (F)"))
                    )
                    .ToString():
                    ""
                    ;
                return sb;
            } 
        }

    }
}
