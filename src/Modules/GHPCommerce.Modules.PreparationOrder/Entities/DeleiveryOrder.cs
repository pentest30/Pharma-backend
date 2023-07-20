using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.PreparationOrder.Entities
{
    public class DeleiveryOrder  : AggregateRoot<Guid>, IEntitySequenceNumber
    {
          public DeleiveryOrder()
        {
            DeleiveryOrderItems = new List<DeleiveryOrderItem>();
        }
        public Guid OrderId { get; set; }      

        /// <summary>
        /// gets or sets order date
        /// </summary>
        public DateTime DeleiveryOrderDate { get; set; }
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// gets or sets customer id
        /// </summary>
        public Guid CustomerId { get; set; }
        public Guid OrganizationId { get; set; }

        /// <summary>
        /// gets or sets customer's name
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// gets or sets supplier id
        /// </summary>
        public Guid SupplierId { get; set; }
        public int TotalPackage { get; set; }
        public int TotalPackageThermolabile { get; set; }
        public string DeleiveryOrderNumber => "BL-"+DeleiveryOrderDate.Date.ToString("yy-MM-dd").Substring(0,2)
                                     +"/" +"0000000000".Substring(0,10-SequenceNumber.ToString().Length)+ SequenceNumber;
     
        /// <summary>
        /// gets or sets deleivery order status
        /// validated == true, canceled == false
        /// </summary>
        public bool Validated { get; set; }
        public string CreatedBy{ get; set; }
        public string UpdatedBy { get; set; }

        public List<DeleiveryOrderItem> DeleiveryOrderItems { get; set; }

        public int SequenceNumber { get; set; }
        public string OrderIdentifier { get; set; }
        public string CodeAx { get; set; }

    }
}