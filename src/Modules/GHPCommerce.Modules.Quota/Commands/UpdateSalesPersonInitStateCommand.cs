using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Quota.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Modules.Quota.Commands
{
    public class UpdateSalesPersonInitStateCommand : ICommand<ValidationResult>
    {
        public Guid OldSalesPersonId { get; set; }
        public Guid SalesPersonId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime DateTime { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid ProductId { get; set; }
    }
    public class UpdateSalesPersonInitStateCommandHandler : ICommandHandler<UpdateSalesPersonInitStateCommand, ValidationResult>
    {
        private readonly IRepository<QuotaInitState, Guid> _repository;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public UpdateSalesPersonInitStateCommandHandler(IRepository<QuotaInitState, Guid> repository, IMapper mapper, ICommandBus commandBus)
        {
            _repository = repository;
            _mapper = mapper;
            _commandBus = commandBus;
        }

        public async Task<ValidationResult> Handle(UpdateSalesPersonInitStateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingEntity = await _repository.Table
                    .Where(x =>
                        x.OrganizationId == request.OrganizationId &&
                        x.DistributionDate.Date == request.DateTime.Date
                        && x.ProductId == request.ProductId && x.QuotaId == request.OldSalesPersonId).ToListAsync(cancellationToken);
                if (existingEntity != null)
                {
                    foreach(var entity in existingEntity)
                    {
                        entity.QuotaId = request.SalesPersonId;
                    }
                    await _repository.UnitOfWork.SaveChangesAsync();

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return default;
            }

            return default;
        }
    }
}
