using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using Microsoft.AspNetCore.Authorization;

namespace GHPCommerce.IS4Admin.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Membership.Roles.Queries;
    using Application.Membership.Users.Commands;
    using Application.Membership.Users.Queries;
    using CrossCuttingConcerns.OS;
    using GHPCommerce.Domain.Domain.Commands;
    using GHPCommerce.Domain.Domain.Identity;
    using Models.UserModels;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

//    [Authorize(Roles = "SuperAdmin,Admin")]
    public class UserController : Controller
    {
       
        private readonly ICommandBus _commandBus;
        private readonly UserManager<User> _userManager;
        private readonly IDateTimeProvider _dateTimeProvider;

        public UserController(ICommandBus commandBus,
            UserManager<User> userManager,
            ILogger<UserController> logger,
            IDateTimeProvider dateTimeProvider)
        {
           
            _commandBus = commandBus;
            _userManager = userManager;
            _dateTimeProvider = dateTimeProvider;
            logger.LogInformation("UserController");
        }

        public async Task<IActionResult> Index()
        {
            var users = await _commandBus.SendAsync(new GetUsersQuery { AsNoTracking = true });
            return View(users);
        }

        public async Task<IActionResult> Profile(Guid id)
        {
            var user = id != Guid.Empty
                ? await _commandBus.SendAsync(new GetUserQuery { Id = id, AsNoTracking = true })
                : new User();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(User model)
        {
            User user;
            if (model.Id != Guid.Empty)
            {
                user = await _commandBus.SendAsync(new GetUserQuery { Id = model.Id });
            }
            else
            {
                user = new User();
            }

            user.UserName = model.UserName;
            user.NormalizedUserName = model.UserName.ToUpper();
            user.Email = model.Email;
            user.NormalizedEmail = model.Email.ToUpper();
            user.EmailConfirmed = model.EmailConfirmed;
            user.PhoneNumber = model.PhoneNumber;
            user.PhoneNumberConfirmed = model.PhoneNumberConfirmed;
            user.TwoFactorEnabled = model.TwoFactorEnabled;
            user.LockoutEnabled = model.LockoutEnabled;
            user.LockoutEnd = model.LockoutEnd;
            user.AccessFailedCount = model.AccessFailedCount;

            _ = model.Id != Guid.Empty
                 ? await _userManager.UpdateAsync(user)
                 : await _userManager.CreateAsync(user);

            return RedirectToAction(nameof(Profile), new { user.Id });
        }

        public async Task<IActionResult> ChangePassword(Guid id)
        {
            var user = await _commandBus.SendAsync(new GetUserQuery { Id = id, AsNoTracking = true });
            return View(ChangePasswordModel.FromEntity(user));
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _commandBus.SendAsync(new GetUserQuery { Id = id, AsNoTracking = true });
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(User model)
        {
            await _commandBus.Send(new DeleteUserCommand { Id = model.Id });
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _commandBus.SendAsync(new GetUserQuery { Id = model.Id });
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var rs = await _userManager.ResetPasswordAsync(user, token, model.ConfirmPassword);

            if (rs.Succeeded)
            {
                return RedirectToAction(nameof(Profile), new { model.Id });
            }

            foreach (var error in rs.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(ChangePasswordModel.FromEntity(user));
        }

        public async Task<IActionResult> Claims(Guid id)
        {
            var user = await _commandBus.SendAsync(new GetUserQuery { Id = id, IncludeClaims = true, AsNoTracking = true });
            return View(ClaimsModel.FromEntity(user));
        }

        [HttpPost]
        public async Task<IActionResult> Claims(ClaimModel model)
        {

            await _commandBus.Send(new AddClaimCommand
            {
                Id = model.User.Id,
                Claim = new UserClaim
                {
                    Type = model.Type,
                    Value = model.Value,
                },
            });

            return RedirectToAction(nameof(Claims), new { id = model.Id});
        }

        public async Task<IActionResult> DeleteClaim(Guid userId, Guid claimId)
        {
            var user = await _commandBus.SendAsync(new GetUserQuery { Id = userId, IncludeClaims = true, AsNoTracking = true });
            var claim = user.Claims.FirstOrDefault(x => x.Id == claimId);

            return View(ClaimModel.FromEntity(claim));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClaim(ClaimModel model)
        {
            var user = await _commandBus.SendAsync(new GetUserQuery
                {Id = model.User.Id, IncludeClaims = true, AsNoTracking = true});

            var claim = user.Claims.FirstOrDefault(x => x.Id == model.Id);
            await _commandBus.Send(new DeleteClaimCommand
            {
                Id = user.Id,
                Claim = claim,
            });

            return RedirectToAction(nameof(Claims), new {id = user.Id});
        }

        public async Task<IActionResult> Roles(Guid id)
        {
            var user = await _commandBus.SendAsync(new GetUserQuery { Id = id, IncludeRoles = true, AsNoTracking = true });

            var roles = await _commandBus.SendAsync(new GetRolesQuery { AsNoTracking = true });

            var model = new RolesModel
            {
                User = user,
                UserRoles = user.UserRoles.Select(x => new RoleModel { Role = x.Role, RoleId = x.RoleId }).ToList(),
                Roles = roles.Where(x => user.UserRoles.All(y => y.RoleId != x.Id)).ToList(),
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Roles(RolesModel model)
        {
            await _commandBus.Send(new AddUserRoleCommand
            {
                Id = model.User.Id,
                Role = new UserRole
                {
                    RoleId = model.Role.RoleId,
                },
            });

            return RedirectToAction(nameof(Roles), new {model.User.Id});
        }

        public async Task<IActionResult> DeleteRole(Guid id, Guid roleId)
        {
            var user = await _commandBus.SendAsync(new GetUserQuery { Id = id, IncludeRoles = true, AsNoTracking = true });
            var role = user.UserRoles.FirstOrDefault(x => x.RoleId == roleId);
            var model = new RoleModel { User = user, Role = role.Role };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(RoleModel model)
        {
            var user = await _commandBus.SendAsync(new GetUserQuery { Id = model.User.Id, IncludeRoles = true, AsNoTracking = true });

            var role = user.UserRoles.FirstOrDefault(x => x.RoleId == model.Role.Id);

            await _commandBus.Send(new DeleteUserRoleCommand
            {
                Id = user.Id,
                Role = role,
            });

            return RedirectToAction(nameof(Roles), new { model.User.Id });
        }
    }
}