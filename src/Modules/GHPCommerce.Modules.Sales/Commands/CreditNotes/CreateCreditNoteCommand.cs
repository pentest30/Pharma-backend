using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Enums;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Sales.Entities.CreditNotes;
using GHPCommerce.Modules.Sales.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using GHPCommerce.Modules.Sales.DTOs.CreditNotes;

namespace GHPCommerce.Modules.Sales.Commands.CreditNotes
{
    public class CreateCreditNoteCommand : ICommand<ValidationResult>
    {
        public int? TotalPackageThermolabile { get; set; }
        public int? TotalPackage { get; set; }

        public Guid InvoiceId { get; set; }
        public List<CreditNoteItemDto> CreditNoteItems { get; set; }
        #region Claim information
        public bool ProductReturn { get; set; } = true;
        public string ClaimNumber { get; set; }
        public string ClaimNote { get; set; }
        public ClaimReasons? ClaimReason { get; set; }
        public DateTime? ClaimDate { get; set; }
        #endregion
    }
    public class CreateCreditNoteCommandValidator : AbstractValidator<CreateCreditNoteCommand>
    {

        public CreateCreditNoteCommandValidator()
        {
           
            RuleFor(v => v.InvoiceId).NotEmpty();
           
        }
       

    }
}