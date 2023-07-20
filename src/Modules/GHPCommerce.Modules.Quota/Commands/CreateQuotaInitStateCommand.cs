using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Quota.Entities;
using GHPCommerce.Modules.Quota.Repositories;
using GreenPipes;

namespace GHPCommerce.Modules.Quota.Commands
{
    public class CreateQuotaInitStateCommand : ICommand<ValidationResult>
    {
        public CreateQuotaInitStateCommand()
        {
            
        }
        public CreateQuotaInitStateCommand(CreateQuotaCommand quotaCommand, Guid? organizationId)
        {
            Quantity = quotaCommand.InitialQuantity;
            CustomerId = quotaCommand.CustomerId;
            CustomerCode = quotaCommand.CustomerCode;
            CustomerName = quotaCommand.CustomerName;
            DistributionDate = quotaCommand.QuotaDate.ToLocalTime().Date;
#if DEBUG
         if(quotaCommand.SalesPersonId == Guid.Empty)
             Console.WriteLine(quotaCommand.SalesPersonId);
#endif
            QuotaId =  quotaCommand.SalesPersonId;
            ProductId = quotaCommand.ProductId;
            if (organizationId != null) OrganizationId = organizationId.Value;
        }
        public Guid CustomerId { get; set; }
        public int Quantity { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
        public DateTime DistributionDate { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid QuotaId { get; set; }
        public Guid ProductId { get; set; }
    }

    public class CreateQuotaInitStateCommandHandler : ICommandHandler<CreateQuotaInitStateCommand, ValidationResult>, ICommandHandler<CreateQuotaInitStateCommands, ValidationResult>
    {
        private readonly Func<QuotaDbContext>_repository;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentOrganization _currentOrganization;

        public CreateQuotaInitStateCommandHandler(Func<QuotaDbContext> repository,   
            IMapper mapper,
            ICommandBus commandBus, ICurrentOrganization currentOrganization)
        {
            _repository = repository;
            _mapper = mapper;
            _commandBus = commandBus;
            _currentOrganization = currentOrganization;
        }
        private async Task UpdateQuotaInitState(int initialQuantity, DateTime quotaDate, Guid customerId, Guid organizationId, Guid productId, CancellationToken cancellationToken)
        {
           
            var initState = new UpdateQuotaInitStateCommand();
            initState.Quantity = initialQuantity;
            initState.CustomerId = customerId;
            initState.OrganizationId = organizationId;
            initState.DateTime = quotaDate;
            initState.ProductId = productId;
            await _commandBus.SendAsync(initState, cancellationToken);
        }
        public async Task<ValidationResult> Handle(CreateQuotaInitStateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                using (var ctx = _repository.Invoke())
                {
                    var entity = _mapper.Map<QuotaInitState>(request);
                    // entity.OrganizationId = organizationId.Value;
                    ctx.Set<QuotaInitState>().Add(entity);
                    await ctx.SaveChangesAsync();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return default;
            }

            return default;
        }

        public async Task<ValidationResult> Handle(CreateQuotaInitStateCommands request, CancellationToken cancellationToken)
        {
            try
            {
                using (var ctx = _repository.Invoke())
                {
                    foreach (var createQuotaInitStateCommand in request.CreateQuotaInitStateCommandList)
                    {
                        var entity = _mapper.Map<QuotaInitState>(createQuotaInitStateCommand);
                        // entity.OrganizationId = organizationId.Value;
                        ctx.Set<QuotaInitState>().Add(entity);
                    }
                   
                    await ctx.SaveChangesAsync();
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

    public class CreateQuotaInitStateCommands : ICommand<ValidationResult>
    {
        public CreateQuotaInitStateCommands()
        {
            CreateQuotaInitStateCommandList = new List<CreateQuotaInitStateCommand>();
        }
        public List<CreateQuotaInitStateCommand> CreateQuotaInitStateCommandList { get; set; }
    }
}