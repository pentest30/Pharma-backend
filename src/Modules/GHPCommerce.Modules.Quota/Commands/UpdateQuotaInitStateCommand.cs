using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Quota.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Quota.Commands
{
    public class UpdateQuotaInitStateCommand : ICommand<ValidationResult>
    {
        public Guid CustomerId { get; set; }
        public int Quantity { get; set; }
        public DateTime DateTime { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid ProductId { get; set; }
    }

    public class UpdateQuotaInitStateCommandHandler : ICommandHandler<UpdateQuotaInitStateCommand, ValidationResult>
    {
        private readonly IRepository<QuotaInitState, Guid> _repository;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public UpdateQuotaInitStateCommandHandler(IRepository<QuotaInitState, Guid> repository,IMapper mapper, ICommandBus commandBus)
        {
            _repository = repository;
            _mapper = mapper;
            _commandBus = commandBus;
        }
      
        public async Task<ValidationResult> Handle(UpdateQuotaInitStateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingEntity = await _repository.Table
                    .FirstOrDefaultAsync(x =>
                        x.CustomerId == request.CustomerId && x.OrganizationId == request.OrganizationId &&
                        x.DistributionDate.Date == request.DateTime.Date 
                        && x.ProductId == request.ProductId, cancellationToken: cancellationToken);
                if (existingEntity != null)
                {
                    existingEntity.Quantity += request.Quantity;
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