using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Application.Membership.Roles.DTOs;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace GHPCommerce.Application.Membership.Roles.Queries
{
    public class RolesQueriesHandler:
        ICommandHandler<GetRolesQuery, IEnumerable<Role>>,
        ICommandHandler<GetRoleQuery, Role>,
        ICommandHandler<GetRoleListQuery, PagingResult<RoleDto>>

    {
        private readonly IRoleRepository _roleRepository;

        public RolesQueriesHandler(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }
        public async Task<IEnumerable<Role>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            var db = _roleRepository.Get(new RoleQueryOptions
            {
                IncludeClaims = request.IncludeClaims,
                IncludeUserRoles = request.IncludeUserRoles,
                AsNoTracking = request.AsNoTracking,
            });
            if (request.ExcludedRoles != null && request.ExcludedRoles.Length > 0)
            {
                db = request.ExcludedRoles.Aggregate(db, (current, requestExcludedRole) => current.Where(x =>! x.Name .Contains( requestExcludedRole)));
            }
            return await db.ToListAsync(cancellationToken);
        }

        public async Task<Role> Handle(GetRoleQuery request, CancellationToken cancellationToken)
        {
            var db = _roleRepository.Get(new RoleQueryOptions
            {
                IncludeClaims = request.IncludeClaims,
                IncludeUserRoles = request.IncludeUserRoles,
                IncludeUsers = request.IncludeUsers,
                AsNoTracking = request.AsNoTracking,
            });

            return await db.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
        }

        public async Task<PagingResult<RoleDto>> Handle(GetRoleListQuery request, CancellationToken cancellationToken)
        {
            var total = await _roleRepository.Table.CountAsync(cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "Name " + request.SortDir : request.SortProp + " " + request.SortDir;
            var query = _roleRepository.Get(new RoleQueryOptions())
                .OrderBy(orderQuery)
                .Paged(request.Page, request.PageSize);
                
            if (!string.IsNullOrEmpty(request.Term))
            {
                query = query
                    .Where(x => x.Name.ToLower().Contains(request.Term.ToLower()));
            }
            var roles = await query.ToListAsync(cancellationToken: cancellationToken);
            var data = roles.Select(x => new RoleDto(x));
            return new PagingResult<RoleDto> { Data = data, Total = total };

        }
    }
}
