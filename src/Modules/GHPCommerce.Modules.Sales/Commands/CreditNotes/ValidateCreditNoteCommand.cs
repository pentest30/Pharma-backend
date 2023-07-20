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
    public class ValidateCreditNoteCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
    }
    public class ValidateCreditNoteCommandValidator : AbstractValidator<ValidateCreditNoteCommand>
    {

        public ValidateCreditNoteCommandValidator()
        {
           
            RuleFor(v => v.Id).NotEmpty();
           
        }
       

    }
}