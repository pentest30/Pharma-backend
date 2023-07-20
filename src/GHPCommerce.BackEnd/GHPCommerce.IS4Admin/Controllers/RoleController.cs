using Microsoft.AspNetCore.Authorization;

namespace GHPCommerce.IS4Admin.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Membership.Roles.Commands;
    using Application.Membership.Roles.Queries;
    using GHPCommerce.Domain.Domain.Commands;
    using GHPCommerce.Domain.Domain.Identity;
    using Models.RoleModels;
    using Microsoft.AspNetCore.Mvc;

  //  [Authorize(Roles = "SuperAdmin,Admin")]
    public class RoleController : Controller
    {
        private readonly ICommandBus _commandBus;
       
        public RoleController(ICommandBus commandBus)
        {
            _commandBus = commandBus;
           
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _commandBus.SendAsync(new GetRolesQuery { AsNoTracking = true });
            return View(roles);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            Role role;
            if (id == Guid.Empty)
                role = new Role();
            else
                role = await _commandBus.SendAsync(new GetRoleQuery { Id = id, AsNoTracking = true });
            var model = role;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Role model)
        {
            Role role;

            if (model.Id == Guid.Empty)
            {
                role = new Role
                {
                    Name = model.Name,
                    NormalizedName = model.Name.ToUpper(),
                };

                await _commandBus.Send(new AddUpdateRoleCommand { Role = role });
            }
            else
            {
                role = await _commandBus.SendAsync(new GetRoleQuery { Id = model.Id });
                role.Name = model.Name;
                role.NormalizedName = model.Name.ToUpper();
                await _commandBus.Send(new AddUpdateRoleCommand { Role = role });
            }

            return RedirectToAction(nameof(Edit), new { role.Id });
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var role = await _commandBus.SendAsync(new GetRoleQuery { Id = id, AsNoTracking = true });
            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Role model)
        {
            await _commandBus.Send(new DeleteRoleCommand { Id = model.Id });

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Claims(Guid id)
        {
            var role = await _commandBus.SendAsync(new GetRoleQuery { Id = id, IncludeClaims = true, AsNoTracking = true });

            return View(ClaimsModel.FromEntity(role));
        }

        [HttpPost]
        public async Task<IActionResult> Claims(ClaimModel model)
        {
            var claim = new RoleClaim
            {
                Type = model.Type,
                Value = model.Value,
            };

            await _commandBus.Send(new AddRoleClaimCommand { Id = model.Role.Id, Claim = claim });

            return RedirectToAction(nameof(Claims), new { id = model.Role.Id });
        }

        public async Task<IActionResult> DeleteClaim(Guid roleId, Guid claimId)
        {
            var role = await _commandBus.SendAsync(new GetRoleQuery { Id = roleId, IncludeClaims = true, AsNoTracking = true });
            var claim = role.Claims.FirstOrDefault(x => x.Id == claimId);

            return View(ClaimModel.FromEntity(claim));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClaim(ClaimModel model)
        {
            var role = await _commandBus.SendAsync(new GetRoleQuery { Id = model.Role.Id, IncludeClaims = true });

            var claim = role.Claims.FirstOrDefault(x => x.Id == model.Id);

            await _commandBus.Send(new DeleteRoleClaimCommand { Id = role.Id, Claim = claim });

            return RedirectToAction(nameof(Claims), new { id = role.Id });
        }

        public async Task<IActionResult> Users(Guid id)
        {
            var role = await _commandBus.SendAsync(new GetRoleQuery { Id = id, IncludeUsers = true, AsNoTracking = true });

            var users = role.UserRoles.Select(x => x.User).ToList();
            var model = new UsersModel
            {
                Role = role,
                Users = users,
            };

            return View(model);
        }
    }
}