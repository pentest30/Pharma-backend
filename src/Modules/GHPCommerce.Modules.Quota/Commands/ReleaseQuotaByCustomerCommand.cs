using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Quota.Entities;
using GHPCommerce.Modules.Quota.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Quota.Commands
{
    public class ReleaseQuotaByCustomerCommand : ICommand<ValidationResult>
    {
        public Guid SalespersonsSourceId { get; set; }
        public Guid SalespersonsDestinationId { get; set; }
        public string SalespersonsDestination { get; set; }
    }
    public class ReleaseQuotaByCustomerCommandHandler : ICommandHandler<ReleaseQuotaByCustomerCommand, ValidationResult>
    {
        private readonly Func<QuotaDbContext> _repository;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentOrganization _currentOrganization;

        public ReleaseQuotaByCustomerCommandHandler(Func<QuotaDbContext> repository,   
            IMapper mapper,
            ICommandBus commandBus,
            ICurrentOrganization currentOrganization)
        {
            _repository = repository;
            _mapper = mapper;
            _commandBus = commandBus;
            _currentOrganization = currentOrganization;
        }
        public async Task<ValidationResult> Handle(ReleaseQuotaByCustomerCommand request, CancellationToken cancellationToken)
        {

            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            using (var ctx = _repository.Invoke())
            {
                var quotaSrc = await ctx.Set<Entities.Quota>()
                    .Where(x => x.SalesPersonId == request.SalespersonsSourceId
                                && x.OrganizationId == orgId)
                    .ToListAsync(cancellationToken: cancellationToken);

                var initQuotas = await ctx.Set<QuotaInitState>()
                    .Where(x => x.QuotaId == request.SalespersonsSourceId
                                && x.OrganizationId == orgId)
                    .ToListAsync(cancellationToken: cancellationToken);
                if (quotaSrc.Any())
                {
                    ctx.Set<Entities.Quota>().RemoveRange(quotaSrc);
                }

                if (initQuotas.Any())
                {
                    ctx.Set<QuotaInitState>().RemoveRange(initQuotas);
                }

                await ctx.SaveChangesAsync();
            }

            return default;
        }
    }
}