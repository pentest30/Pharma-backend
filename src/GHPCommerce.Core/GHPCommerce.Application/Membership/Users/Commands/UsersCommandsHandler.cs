using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Persistence;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;

namespace GHPCommerce.Application.Membership.Users.Commands
{
    public class UsersCommandsHandler:
        ICommandHandler<DeleteUserCommand>,
        ICommandHandler<AddClaimCommand>,
        ICommandHandler<DeleteClaimCommand>,
        ICommandHandler<DeleteUserRoleCommand>,
        ICommandHandler<AddUserRoleCommand>,
        ICommandHandler<AddUserAddressCommand>,
        ICommandHandler<AddUserRoleCommandV1>, ICommandHandler<UpdateUserCommand, ValidationResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IRepository<User, Guid> _repository;
        private readonly ConnectionStrings _connectionStrings;

        public UsersCommandsHandler(IUserRepository userRepository, 
            IRoleRepository roleRepository, 
            IRepository<User, Guid> repository, ConnectionStrings connectionStrings)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _repository = repository;
            _connectionStrings = connectionStrings;
        }
        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository
                .Get(new UserQueryOptions())
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if(user == null)
                throw new NotFoundException($"User with id: {request.Id} was not found");
            _userRepository.Delete(user);
            await _userRepository.UnitOfWork.SaveChangesAsync();
            return Unit.Value;
        }

        public async Task<Unit> Handle(AddClaimCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository
                .Get(new UserQueryOptions())
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (user == null)
                throw new NotFoundException($"User with id: {request.Id} was not found");
            if (user.Claims.Any(x => x.Value == request.Claim.Value && x.Type == request.Claim.Type)) return Unit.Value;
            user.Claims.Add(request.Claim);
            _userRepository.AddOrUpdate(user);
            await _userRepository.UnitOfWork.SaveChangesAsync();
            return  Unit.Value;
        }

        public async Task<Unit> Handle(DeleteClaimCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository
                .Get(new UserQueryOptions() {IncludeClaims = true})
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (user == null)
                throw new NotFoundException($"User with id: {request.Id} was not found");
            var claim = user.Claims.FirstOrDefault(x => x.Value == request.Claim.Value && x.Type == request.Claim.Type);
            user.Claims.Remove(claim);
            _userRepository.AddOrUpdate(user);
            await _userRepository.UnitOfWork.SaveChangesAsync();
            return Unit.Value;
        }
        public async Task<Unit> Handle(DeleteUserRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository
                .Get(new UserQueryOptions { IncludeUserRoles = true })
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (user == null)
                throw new NotFoundException($"User with id: {request.Id} was not found");
            var role = user.UserRoles.FirstOrDefault(x => x.RoleId == request.Role.RoleId);
            user.UserRoles.Remove(role);
            _userRepository.Update(user);
            await _userRepository.UnitOfWork.SaveChangesAsync();
            return Unit.Value;
        }
        public async Task<Unit> Handle(AddUserRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository
                .Get(new UserQueryOptions { IncludeUserRoles = true})
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (user == null)
                throw new NotFoundException($"User with id: {request.Id} was not found");
            if (user.UserRoles.Any(x => x.RoleId == request.Role.RoleId)) return Unit.Value;
            user.UserRoles.Add(request.Role);
            _userRepository.AddOrUpdate(user);
            await _userRepository.UnitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<Unit> Handle(AddUserAddressCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository
                .Get(new UserQueryOptions {IncludeAddresses = true})
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            
            if (user == null)
                throw new NotFoundException($"User with id: {request.Id} was not found");
            user.Addresses.Add(request.Address);
            _userRepository.AddOrUpdate(user);
            await _userRepository.UnitOfWork.SaveChangesAsync();
            return Unit.Value;
        }

        public async Task<Unit> Handle(AddUserRoleCommandV1 request, CancellationToken cancellationToken)
        {
            var user = await _userRepository
                .Get(new UserQueryOptions {IncludeRoles = true})
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (user == null)
                throw new NotFoundException($"User with id: {request.Id} was not found");

            var role = await _roleRepository.Table.FirstOrDefaultAsync(x => x.Name == request.RoleName,
                cancellationToken);
            if (role == null)
                throw new NotFoundException($"Role with name: {request.RoleName} was not found");
            user.UserRoles.Add(new UserRole() {UserId =  request.Id, RoleId =  role.Id});
            _userRepository.AddOrUpdate(user);
            await _userRepository.UnitOfWork.SaveChangesAsync();
            return Unit.Value;
        }

        public async Task<ValidationResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            using (IDbConnection db = new SqlConnection(_connectionStrings.ConnectionString))
            {
                
                
                string updateQuery = @"UPDATE [ids].[Users]
	                                SET [UserName] = @UserName
		                                ,[NormalizedUserName] = @NormalizedUserName
		                                ,[Email] = @Email
		                                ,[NormalizedEmail] = @NormalizedEmail
		                                ,[EmailConfirmed] = @EmailConfirmed
		                                ,[PhoneNumber] = @PhoneNumber
		                                ,[PhoneNumberConfirmed] = @PhoneNumberConfirmed
		                                ,[OrganizationId] = @OrganizationId
		                                ,[ManagerId] = @ManagerId
		                                ,[Company] = @Company
		                                ,[FirstName] = @FirstName
		                                ,[LastName] = @LastName
                                    where id = @Id";

                try
                {
                    await db.ExecuteAsync(updateQuery, request.User);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return new ValidationResult
                        { Errors = { new ValidationFailure("Error DB", e.Message) } };

                    
                }
            }
            // var user = await _repository.Table
            //     //.AsNoTracking()
            //     .Where(x => x.Id == request.User.Id)
            //     .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            // if (user == null)
            //     return default;
            // try
            // {
            //     user = request.User.ShallowClone();
            //     _repository.Update(user);
            //     await _repository.UnitOfWork.SaveChangesAsync();
            // }
            // catch (Exception e)
            // {
            //     Console.WriteLine(e);
            //     throw;
            // }
            return  default;
        }
    }
}
