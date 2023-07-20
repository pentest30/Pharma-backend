using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Application.Tiers.Sectors.DTOs;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Tiers.Sectors.Queries
{
    public class SectorCustomerQueriesHandler :
        ICommandHandler<CheckSectorByNameQuery, bool>,
        ICommandHandler<GetSectorsListQuery, PagingResult<SectorDto>>,
        ICommandHandler<GetAllSectorsQuery, IEnumerable<SectorDto>>,
        ICommandHandler<GetSectorIdByCodeQuery, Guid>,
        ICommandHandler<GetPagedSectorsList, SyncPagedResult<SectorDto>>
    {
        private readonly IRepository<SectorCustomer, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;

        public SectorCustomerQueriesHandler(IRepository<SectorCustomer, Guid> repository, ICurrentOrganization currentOrganization)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
        }
        public Task<bool> Handle(CheckSectorByNameQuery request, CancellationToken cancellationToken)
        {
            return _repository.Table.AnyAsync(x => x.Code != request.Code && x.Name == request.Name, cancellationToken);
        }

        public async Task<PagingResult<SectorDto>> Handle(GetSectorsListQuery request, CancellationToken cancellationToken)
        {
            var total = await _repository.Table.CountAsync(cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "Name " + request.SortDir : request.SortProp + " " + request.SortDir;

            var query = await _repository
                .Table
                .Include(x => x.Organization)
                .OrderBy(orderQuery)
                .Where(x => string.IsNullOrEmpty(request.Term)
                            || x.Name.ToLower().Contains(request.Term.ToLower())
                            || x.Code.ToLower().Contains(request.Term.ToLower()))


                                .Select(x => new SectorDto { Code = x.Code, Id = x.Id, Name = x.Name, Organization = x.Organization.Name, OrganizationId = x.OrganizationId })

                                .Paged(request.Page, request.PageSize)
                                .ToListAsync(cancellationToken);

            return new PagingResult<SectorDto> { Data = query, Total = total };

        }

        public async Task<IEnumerable<SectorDto>> Handle(GetAllSectorsQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == Guid.Empty)
                throw new InvalidOperationException("Organization id cannot be null or empty");

            var query = await _repository
                .Table
                .Where(x => x.OrganizationId == orgId)
                .Select(x => new SectorDto { Id = x.Id, Name = x.Name })
                .ToListAsync(cancellationToken);
            return query;
        }

        public Task<Guid> Handle(GetSectorIdByCodeQuery request, CancellationToken cancellationToken)
        {
            return _repository.Table.Where(x => x.Code == request.Code).Select(x => x.Id).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }

        public async Task<SyncPagedResult<SectorDto>> Handle(GetPagedSectorsList request, CancellationToken cancellationToken)
        {

            var query = _repository.Table;
            query = query.DynamicWhereQuery(request.GridQuery);
            var total = await query.CountAsync(cancellationToken);
            query = query
                    .Paged(request.GridQuery.Skip / request.GridQuery.Take + 1, request.GridQuery.Take);
            var data = await query
                .Select(x => new SectorDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code
                })
                .ToListAsync(cancellationToken);
            return new SyncPagedResult<SectorDto> { Result = data, Count = total };
        }
    }
}
