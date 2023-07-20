using System;
using System.Collections.Generic;

namespace GHPCommerce.Modules.Sales.DTOs.Invoices
{
    public class DebtDto
    {
        
        public string Company { get; set; }
        public string CustomerCode { get; set; }
        public int InvoiceMonth { get; set; }
        public int InvoiceYear { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal Dept { get; set; }

    }

    public class DebtDetailResultDto
    {
        public List<DebtDetailDto> Data { get; set; }
        public int TotalItems { get; set; }
    } 
    public class DebtDetailDto
    {
        public string Company { get; set; }
        public string CustomerCode { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal Dept { get; set; }
        public int InvoiceMonth { get; set; }
        public int InvoiceYear { get; set; }
        public string CustomerName { get; set; }

    }

}