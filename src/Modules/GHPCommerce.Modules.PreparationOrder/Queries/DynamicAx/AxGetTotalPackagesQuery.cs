using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.PreparationOrder.DTOs;
using GHPCommerce.Modules.PreparationOrder.Entities;
using GHPCommerce.Modules.PreparationOrder.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.PreparationOrder.Queries.DynamicAx
{
    public class AxGetTotalPackagesQuery : ICommand<ConsolidationOrderAxDtoV1>
    {
        public string AxNumber { get; set; }
    }

    public class AxGetTotalPackagesQueryHandler : ICommandHandler<AxGetTotalPackagesQuery, ConsolidationOrderAxDtoV1>
    {
        private readonly IRepository<ConsolidationOrder, Guid> _repository;
        private readonly IRepository<Entities.PreparationOrder, Guid> _preparationOrderRepository;

        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICurrentUser _currentUser;
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;
        private readonly ICache _cache;
        public AxGetTotalPackagesQueryHandler(
            IRepository<ConsolidationOrder, Guid> repository,
            IRepository<Entities.PreparationOrder, Guid> preparationOrderRepository,
            ICurrentOrganization currentOrganization,
            ICurrentUser currentUser,
            ICommandBus commandBus,
            ICache cache,
            IMapper mapper)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _commandBus = commandBus;
            _mapper = mapper;
            _cache = cache;

        }

        public async Task<ConsolidationOrderAxDtoV1> Handle(AxGetTotalPackagesQuery request, CancellationToken cancellationToken)
        {

            var orderId =  await _cache.GetAsync<Guid>("_order"+request.AxNumber, cancellationToken);
            var consolidationOrder = await _repository.Table.Where(c => c.OrderId == orderId).FirstOrDefaultAsync(cancellationToken);
            if (consolidationOrder != null)
                return new ConsolidationOrderAxDtoV1()
                {
                    TotalPackageAmbiant = consolidationOrder.TotalPackage,
                    TotalPackageThermolabile = consolidationOrder.TotalPackageThermolabile
                };
            return default;


        }
    }

}