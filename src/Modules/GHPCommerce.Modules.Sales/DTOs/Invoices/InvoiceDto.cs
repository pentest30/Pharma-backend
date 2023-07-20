using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Enums;
using GHPCommerce.Modules.Sales.Entities.Billing;

namespace GHPCommerce.Modules.Sales.DTOs.Invoices
{
    public class InvoiceDto
    {
        public Guid Id { get; set; }
        
        public Guid OrganizationId { get; set; }
        
        public Guid InvoiceId { get; set; }

        public Guid OrderId { get; set; }
        
        public Guid DeliveryOrderId { get; set; }
        public DateTime InvoiceDate { get; set; }
        
        public DateTime OrderDate { get; set; }
        
        public int OrderNumber { get; set; }
        
        public Guid CustomerId { get; set; }
        
        public string CustomerName { get; set; }
        
        public string CustomerAddress { get; set; }
        
        public string CustomerCode { get; set; }

        public Guid SupplierId { get; set; }
        
        public string InvoiceNumber => "FC-"+InvoiceDate.Date.ToString("yy-MM-dd").Substring(0,2)
                                            +"/" +"0000000000".Substring(0,10-SequenceNumber.ToString().Length)+ SequenceNumber;
        public int SequenceNumber { get; set; }
        
        public int TotalPackage { get; set; }
        
        public int TotalPackageThermolabile { get; set; }

        public OrderType InvoiceType { get; set; }
        
        public decimal TotalTTC { get; set; }
        
        public decimal TotalHT { get; set; }
        
        public decimal TotalDiscount { get; set; }
        
        public decimal TotalDisplayedDiscount { get; set; }

        public decimal TotalTax { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public DateTimeOffset? UpdatedDateTime { get; set; }

        public Guid UpdatedByUserId { get; set; }
        
        
        public string SectorCode { get; set; }
        
        public string Sector { get; set; }
        
        public string CodeRegion { get; set; }
        
        public string Region { get; set; }
       
        public DateTime DueDate { get; set; }
        
        public int NumberDueDays { get; set; }

        public int NumberOfPrints { get; set; }
        
        public Guid PrintedBy { get; set; }
        
        public string PrintedByName { get; set; }

        public List<InvoiceItemDto> InvoiceItems { get; set; }
    }
}