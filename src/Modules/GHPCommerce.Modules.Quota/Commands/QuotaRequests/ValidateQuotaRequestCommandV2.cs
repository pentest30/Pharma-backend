using System;
using System.Collections.Generic;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Quota.Commands.QuotaRequests
{
    public class ValidateQuotaRequestCommandV2 : ICommand<ValidationResult>
    {
        public Guid RequestId { get; set; }
        public List<QuotaDetail> QuotaDetails { get; set; }
    }

    public class QuotaDetail
    {
        public Guid CustomerId { get; set; }
        public int Quantity { get; set; }
    }
}