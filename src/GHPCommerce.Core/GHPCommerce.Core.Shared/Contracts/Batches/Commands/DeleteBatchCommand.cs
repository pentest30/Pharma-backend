using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Batches.Commands
{
    public class DeleteBatchCommand : ICommand<ValidationResult>
    {
        public Guid BatchId { get; set; }
    }
}