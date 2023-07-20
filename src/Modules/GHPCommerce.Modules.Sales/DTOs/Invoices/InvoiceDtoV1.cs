using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Enums;

namespace GHPCommerce.Modules.Sales.DTOs.Invoices
{
    public class InvoiceDtoV1
    {
        public Guid Id { get; set; }
        
        public DateTime InvoiceDate { get; set; }
        
        public DateTime OrderDate { get; set; }
        
        public int OrderNumber { get; set; }
        
        public Guid CustomerId { get; set; }
        
        public string CustomerName { get; set; }
        
        public string CustomerAddress { get; set; }
        
        public string CustomerCode { get; set; }
        
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
        public List<InvoiceItemDto> InvoiceItems { get; set; }
    }
}