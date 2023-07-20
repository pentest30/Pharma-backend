using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Membership.Users.DTOs;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using MassTransit.Transports;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Membership.Users.Queries
{
    public class UsersQueriesHandler :
        ICommandHandler<GetUsersQuery, IEnumerable<User>>,
        ICommandHandler<GetUserQuery, User>, 
        ICommandHandler<GetUsersListQuery, PagingResult<UserDto>>,
        ICommandHandler<GetSalePersonsQuery, IEnumerable<UserDtoV1>>,
        ICommandHandler<GetExecutersQuery, IEnumerable<UserDtoV1>>,
        ICommandHandler<GetVerifiersQuery, IEnumerable<UserDtoV1>>,
        ICommandHandler<GetSalesPersonIdsBySalesManageQuery, IEnumerable<Guid>>,
        ICommandHandler<GetUserIdByNameQuery, Guid>,
        ICommandHandler<GetSalesPersonsBySupervisorQuery, IEnumerable<Guid>>,
        ICommandHandler<GetSalesPersonsBySupervisorV2Query, IEnumerable<UserDtoV2>>,
        ICommandHandler<GetSupervisorsQuery, IEnumerable<UserDtoV1>>,
        ICommandHandler<GetSupervisorsQueryV2 ,IEnumerable<UserDtoV1>>,
        ICommandHandler<GetConsolidatorsQuery, IEnumerable<UserDtoV1>>,
        ICommandHandler<GetPagedUsersQuery, SyncPagedResult<UserDto>>,
        ICommandHandler<GetOrganizationIdByUserQuery, Guid?>


    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICurrentUser _currentUser;
        private readonly IMapper _mapper;

        public UsersQueriesHandler(IUserRepository userRepository,ICurrentOrganization currentOrganization, 
            ICurrentUser currentUser, IMapper mapper)
        {
            _userRepository = userRepository;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _mapper = mapper;
        }
        public async Task<IEnumerable<User>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
           var users = await _userRepository. Get(new UserQueryOptions
           {
               IncludeClaims = request.IncludeClaims,
               IncludeUserRoles = request.IncludeUserRoles,
               IncludeRoles = request.IncludeRoles,
               AsNoTracking = request.AsNoTracking,
           }).ToListAsync(cancellationToken);
           return users;

        }

        public async Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var user =await _userRepository.Get(new UserQueryOptions
                {
                    IncludeClaims = request.IncludeClaims,
                    IncludeUserRoles = request.IncludeUserRoles,
                    IncludeRoles = request.IncludeRoles,
                    IncludeOrganization = request.IncludeOrganization,
                    AsNoTracking = request.AsNoTracking,
                }).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            
                return user;
            }
            catch (Exception e)
            {
                throw e;
            }
           
        }

        public async Task<PagingResult<UserDto>> Handle(GetUsersListQuery request, CancellationToken cancellationToken) {
            var total = await _userRepository.Table.CountAsync(cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "UserName " + request.SortDir : request.SortProp + " " + request.SortDir;
            var query =  _userRepository.Get(new UserQueryOptions {IncludeRoles = true, IncludeClaims =  true,IncludeOrganization =true})
                .OrderBy(orderQuery)
                .Where(x=>x.Id != _currentUser.UserId)
                .Paged(request.Page, request.PageSize);
            if (!string.IsNullOrEmpty(request.Term))
            {
                query = query
                    .Where(x => x.UserName.ToLower().Contains(request.Term.ToLower())
                                || x.Email.ToLower().Contains(request.Term.ToLower()));
            }

            var isSuperAdmin = (await _userRepository.Get(new UserQueryOptions {IncludeRoles = true})
                .OrderBy(orderQuery)
                .FirstOrDefaultAsync(x => x.Id == _currentUser.UserId,cancellationToken))
                .UserRoles.Any(r=>r.Role.Name.ToLower()
                    == "superadmin");
            var currentOrganizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!isSuperAdmin && currentOrganizationId.HasValue) query = query.Where(x =>  x.OrganizationId == currentOrganizationId.Value);
            var users=  await query.ToListAsync(cancellationToken);
            var data = users.Select(x=> new UserDto(x));
            return new PagingResult<UserDto> { Data = data, Total = total };

        }
        // gets a list of users with role == "SalesPerson"
        public async Task<IEnumerable<UserDtoV1>> Handle(GetSalePersonsQuery request, CancellationToken cancellationToken)
        {
            var currentOrganizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if(!currentOrganizationId.HasValue)
                throw new InvalidOperationException("");
            var query = _userRepository
                .Get(new UserQueryOptions {IncludeRoles = true})
                .Where(x => x.UserRoles.Any(r=>r.Role.Name == "SalesPerson") && x.OrganizationId == currentOrganizationId)
                .Select(u=> new UserDtoV1 {Id = u.Id, Email = u.Email, UserName = u.UserName});
            return await query.ToListAsync(cancellationToken);

        }

        public async Task<IEnumerable<Guid>> Handle(GetSalesPersonIdsBySalesManageQuery request, CancellationToken cancellationToken)
        {
            var query = await _userRepository.Table
                .Where(x => x.ManagerId == request.UserId && x.OrganizationId.Value == request.OrganizationId)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
            return query;
        }

        public async Task<Guid> Handle(GetUserIdByNameQuery request, CancellationToken cancellationToken)
        {
            var query = _userRepository
                .Get(new UserQueryOptions { IncludeRoles = true })
                .Where(x => x.UserName == request.UserName)
                .Select(u =>  u .Id);
            return await query.FirstOrDefaultAsync(cancellationToken);

        }

        public async Task<IEnumerable<Guid>> Handle(GetSalesPersonsBySupervisorQuery request, CancellationToken cancellationToken)
        {
            var query = await _userRepository
                .Table
                .Where(x => x.ManagerId == request.Id)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
            return query;
        }
        public async Task<IEnumerable<UserDtoV2>> Handle(GetSalesPersonsBySupervisorV2Query request, CancellationToken cancellationToken)
        {
            var query = await _userRepository
                .Table
                .Where(x => x.ManagerId == request.Id)
                .Select(u => new UserDtoV2 { Id = u.Id, Email = u.Email, ManagerId = u.ManagerId, UserName = u.UserName })
                .ToListAsync(cancellationToken);
            return query;
        }
        public async Task<IEnumerable<UserDtoV1>> Handle(GetSupervisorsQuery request, CancellationToken cancellationToken)
        {
            var currentOrganizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if(!currentOrganizationId.HasValue)
                return default;
            var query = _userRepository
                .Get(new UserQueryOptions {IncludeRoles = true})
                .Where(x => x.UserRoles.Any(r=>r.Role.Name == "Supervisor") && x.OrganizationId == currentOrganizationId)
                .Select(u=> new UserDtoV1 {Id = u.Id, Email = u.Email, UserName = u.UserName});
            return await query.ToListAsync(cancellationToken);
        }
        public async Task<IEnumerable<UserDtoV1>> Handle(GetSupervisorsQueryV2 request, CancellationToken cancellationToken)
        {
             var query = _userRepository
                .Get(new UserQueryOptions {IncludeRoles = true})
                .Where(x => x.UserRoles.Any(r=>r.Role.Name == "Supervisor") )
                .Select(u=> new UserDtoV1 {Id = u.Id, Email = u.Email, UserName = u.UserName});
            return await query.ToListAsync(cancellationToken);
        }
        public async Task<IEnumerable<UserDtoV1>> Handle(GetExecutersQuery request, CancellationToken cancellationToken)
        {
            var currentOrganizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!currentOrganizationId.HasValue)
                throw new InvalidOperationException("");
            var query = _userRepository
                .Get(new UserQueryOptions { IncludeRoles = true })
                .Where(x => x.UserRoles.Any(r => r.Role.Name == "Executer") && x.OrganizationId == currentOrganizationId)
                .Select(u => new UserDtoV1 { Id = u.Id, Email = u.Email, UserName = u.UserName });
            return await query.ToListAsync(cancellationToken);

        }

        public async Task<IEnumerable<UserDtoV1>> Handle(GetVerifiersQuery request, CancellationToken cancellationToken)
        {
            var currentOrganizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!currentOrganizationId.HasValue)
                throw new InvalidOperationException("");
            var query = _userRepository
                .Get(new UserQueryOptions { IncludeRoles = true })
                .Where(x => x.UserRoles.Any(r => r.Role.Name == "Controller") && x.OrganizationId == currentOrganizationId)
                .Select(u => new UserDtoV1 { Id = u.Id, Email = u.Email, UserName = u.UserName });
            return await query.ToListAsync(cancellationToken);

        }

        public async Task<IEnumerable<UserDtoV1>> Handle(GetConsolidatorsQuery request, CancellationToken cancellationToken)
        {
            var currentOrganizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!currentOrganizationId.HasValue)
                throw new InvalidOperationException("");
            var query = _userRepository
                .Get(new UserQueryOptions { IncludeRoles = true })
                .Where(x => x.UserRoles.Any(r => r.Role.Name == "Consolidator") && x.OrganizationId == currentOrganizationId)
                .Select(u => new UserDtoV1 { Id = u.Id, Email = u.Email, UserName = u.UserName });
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<SyncPagedResult<UserDto>> Handle(GetPagedUsersQuery request, CancellationToken cancellationToken)
        {
            var currentOrganizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var query = _userRepository.Table 
                .Include(x => x.Organization)
                .Include(x=>x.UserRoles)
                .Include("UserRoles.Role")
                .DynamicWhereQuery(request.GridQuery);
            if (request.GridQuery.Where != null )
            {
                foreach (var wherePredicate in request.GridQuery.Where[0].Predicates)
                {
                    if(wherePredicate.Value == null) 
                        continue;
                    if (wherePredicate.Field == "organizationName")
                        query = query .Where(x=>x.Organization.Name.Contains( wherePredicate.Value.ToString()));

                }
            
            }
            var isSuperAdmin = (await _userRepository.Get(new UserQueryOptions {IncludeRoles = true})
                    .FirstOrDefaultAsync(x => x.Id == _currentUser.UserId,cancellationToken))
                .UserRoles.Any(r=>r.Role.Name.ToLower()
                                  == "superadmin");
            if (!isSuperAdmin && currentOrganizationId.HasValue) query = query.Where(x =>  x.OrganizationId == currentOrganizationId.Value);
            var total = await query.CountAsync(cancellationToken: cancellationToken);
            var result = await query
                .Paged(request.GridQuery.Skip / request.GridQuery.Take + 1,
                    request.GridQuery.Take)
                .Select(x => new UserDto(x))
                .ToListAsync(cancellationToken);
            return new SyncPagedResult<UserDto>
                {Result = result, Count = total};
        }

        public async Task<Guid?> Handle(GetOrganizationIdByUserQuery request, CancellationToken cancellationToken)
        {
            var user =await _userRepository.Get(new UserQueryOptions
            {
               
                IncludeOrganization = true,
                AsNoTracking = true,
            })
                .Where(x=>x.Id == request.UserId)
                .Select(x=>x.OrganizationId)
                .FirstOrDefaultAsync( cancellationToken)
                .ConfigureAwait(false);
            return user;
        }
    }
}
