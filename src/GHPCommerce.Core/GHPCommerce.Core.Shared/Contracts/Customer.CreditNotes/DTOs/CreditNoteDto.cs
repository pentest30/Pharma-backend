using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Enums;
using GHPCommerce.Modules.Sales.DTOs.CreditNotes; 

namespace GHPCommerce.Core.Shared.Contracts.Customer.CreditNotes.DTOs
{
    public class CreditNoteDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid SupplierId { get; set; }
        public Guid CreditNoteId  => this.Id;
        #region Claim information
        public bool ProductReturn { get; set; } = true;
        public string ClaimNumber { get; set; }
        public string ClaimNote { get; set; }
        public ClaimReasons? ClaimReason { get; set; }
        public DateTime? ClaimDate { get; set; }
        #endregion

        public DateTime CreditNoteDate { get; set; }

        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }

        public int OrderNumber { get; set; }

        public Guid InvoiceId { get; set; }

        public DateTime InvoiceDate { get; set; }

        public string InvoiceNumber { get; set; }


        public Guid CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerAddress { get; set; }

        public string CustomerCode { get; set; }

        public string CreditNoteNumber => "AV-" + InvoiceDate.Date.ToString("yy-MM-dd").Substring(0, 2)
                                                   + "/" + "0000000000".Substring(0, 10 - SequenceNumber.ToString().Length) + SequenceNumber;
        public int SequenceNumber { get; set; }

        public int TotalPackage { get; set; }

        public int TotalPackageThermolabile { get; set; }

        public OrderType CreditNoteType { get; set; }

        public decimal TotalTTC { get; set; }

        public decimal TotalHT { get; set; }

        public decimal TotalDiscount { get; set; }
        public decimal TotalDisplayedDiscount { get; set; }

        public decimal TotalTax { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

        public string SectorCode { get; set; }

        public string Sector { get; set; }

        public string CodeRegion { get; set; }

        public string Region { get; set; }

        public int NumberOfPrints { get; set; }

        public Guid PrintedBy { get; set; }

        public string PrintedByName { get; set; }

        public List<CreditNoteItemDto> CreditNoteItems { get; set; }
        public Guid UpdatedByUserId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public CreditNoteState State { get; set; } = CreditNoteState.Draft;
        public Guid? ValidatedByUserId { get; set; }
        public DateTime? ValidatedOn { get; set; }
    }
}