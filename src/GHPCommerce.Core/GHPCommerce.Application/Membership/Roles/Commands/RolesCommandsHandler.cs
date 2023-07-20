using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Membership.Roles.Commands
{
    public class RolesCommandsHandler:
        ICommandHandler<AddUpdateRoleCommand>,
        ICommandHandler<DeleteRoleCommand>,
        ICommandHandler<AddRoleClaimCommand>,
        ICommandHandler<DeleteRoleClaimCommand>
    {
        private readonly IRoleRepository _roleRepository;

        public RolesCommandsHandler( IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }
        public async Task<Unit> Handle(AddUpdateRoleCommand request, CancellationToken cancellationToken)
        {
            _roleRepository.AddOrUpdate(request.Role);
            await _roleRepository.UnitOfWork.SaveChangesAsync();
            return Unit.Value;
        }

        public async Task<Unit> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _roleRepository
                .Get(new RoleQueryOptions())
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (role == null)
                throw new NotFoundException($"User with id: {request.Id} was not found");
            _roleRepository.Delete(role);
            await _roleRepository.UnitOfWork.SaveChangesAsync();
            return  Unit.Value;
        }

        public async Task<Unit> Handle(AddRoleClaimCommand request, CancellationToken cancellationToken)
        {
            var role = await _roleRepository
                .Get(new RoleQueryOptions())
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (role == null)
                throw new NotFoundException($"User with id: {request.Id} was not found");
            role.Claims.Add(request.Claim);
            _roleRepository.AddOrUpdate(role);
            await _roleRepository.UnitOfWork.SaveChangesAsync();
            return Unit.Value;
        }

        public async Task<Unit> Handle(DeleteRoleClaimCommand request, CancellationToken cancellationToken)
        {
            var role = await _roleRepository
                .Get(new RoleQueryOptions())
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (role == null)
                throw new NotFoundException($"User with id: {request.Id} was not found");
            role.Claims.Remove(request.Claim);
            _roleRepository.AddOrUpdate(role);
            await _roleRepository.UnitOfWork.SaveChangesAsync();
            return Unit.Value;
        }
    }
}
