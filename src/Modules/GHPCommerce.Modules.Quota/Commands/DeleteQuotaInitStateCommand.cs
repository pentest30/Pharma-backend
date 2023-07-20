using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Quota.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Quota.Commands
{
    public class DeleteQuotaInitStateCommand : ICommand<ValidationResult>
    {
        public Guid SalesPersonId { get; set; }
        public Guid ProductId { get; set; }
        public DateTime QuotaDate { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid CustomerId { get; set; }
    }
    public  class  DeleteQuotaInitStateCommandHandler: ICommandHandler<DeleteQuotaInitStateCommand, ValidationResult>
    {
        private readonly IRepository<QuotaInitState, Guid> _repository;

        public DeleteQuotaInitStateCommandHandler(IRepository<QuotaInitState, Guid> repository)
        {
            _repository = repository;
        }
        public  async Task<ValidationResult> Handle(DeleteQuotaInitStateCommand request, CancellationToken cancellationToken)
        {
            var existingEntity = await _repository.Table
                .Where(x =>
                        x.QuotaId == request.SalesPersonId 
                        && x.OrganizationId == request.OrganizationId
                        &&x.DistributionDate.Date == request.QuotaDate.Date
                        && x.ProductId == request.ProductId
                   ).ToListAsync(cancellationToken: cancellationToken);
            foreach (var quotaInitState in existingEntity)
            {
                _repository.Delete(quotaInitState);
            }

            await _repository.UnitOfWork.SaveChangesAsync();
            return default;
        }
    }
}