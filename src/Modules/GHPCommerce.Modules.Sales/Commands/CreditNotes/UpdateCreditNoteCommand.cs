using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Enums;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.DTOs.CreditNotes;
using System;
using System.Collections.Generic;

namespace GHPCommerce.Modules.Sales.Commands.CreditNotes
{
    public class UpdateCreditNoteCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public int? TotalPackageThermolabile { get; set; }
        public int? TotalPackage { get; set; } 
        public List<CreditNoteItemDto> CreditNoteItems { get; set; }
        #region Claim information
        public bool ProductReturn { get; set; } = true;
        public string ClaimNumber { get; set; }
        public string ClaimNote { get; set; }
        public ClaimReasons? ClaimReason { get; set; }
        public DateTime? ClaimDate { get; set; }
        #endregion
    }
    public class UpdateCreditNoteCommandValidator : AbstractValidator<UpdateCreditNoteCommand>
    {

        public UpdateCreditNoteCommandValidator()
        {

            RuleFor(v => v.Id).NotEmpty();

        }


    }
}