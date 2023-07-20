using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using GHPCommerce.Application.Membership.Users.Commands;
using GHPCommerce.Application.Membership.Users.DTOs;
using GHPCommerce.Application.Membership.Users.Queries;
using GHPCommerce.Application.Tiers.Customers.Queries;
using GHPCommerce.Application.Tiers.Suppliers.Queries;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Domain.Shared;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.Filters;
using GHPCommerce.WebApi.Hepers;
using GHPCommerce.WebApi.Models.Organizations;
using GHPCommerce.WebApi.Models.Users;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    //[Authorize]
    public class UsersController : ApiController
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
       
        private readonly ICommandBus _commandBus;
        private readonly IEmailSender _sender;
        private readonly IConfiguration _configuration;
        public UsersController(
            IMapper mapper, 
            UserManager<User> userManager,
            ICommandBus commandBus, 
            IEmailSender sender, 
            IConfiguration configuration)
        {
           
            _mapper = mapper;
            _userManager = userManager;
            _commandBus = commandBus;
            _sender = sender;
            _configuration = configuration;
        }
        
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.GET, "SuperAdmin", "Admin")]
        public Task<PagingResult<UserDto>> Get(string term, string sort, int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetUsersListQuery(term, sort, page, pageSize));
        }
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("search")]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.GET, "SuperAdmin", "Admin")]
        public Task<SyncPagedResult<UserDto>> GetPaged(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetPagedUsersQuery {GridQuery = query});
        }

        [HttpGet]
        [Route("/api/users/getAll")]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.GET, "SuperAdmin", "Admin")]

        public async Task<IEnumerable<UserModel>> GetAll()
        {
            var users = await _commandBus.SendAsync(new GetUsersQuery());
            var model = _mapper.Map<IEnumerable<UserModel>>(users);
            return model;
        }

        [Route("/api/users/{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.GET, "SuperAdmin", "Admin", "SalesPerson","SalesManage", "Supervisor","OnlineCustomer")]
       
        public async Task<UserModel> Get(Guid id)
        {
            var user = await _commandBus.SendAsync(new GetUserQuery { Id = id, AsNoTracking = true });
            return _mapper.Map<UserModel>(user);
           
        }
        [Route("/api/users/sales-persons")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.GET,  "Admin", "SalesPerson","OnlineCustomer")]

        public  Task<IEnumerable<UserDtoV1>> GetSalesPersons()
        {
            return _commandBus.SendAsync(new GetSalePersonsQuery {IncludeRoles = true});

        }
        [Route("/api/users/sales-persons/{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.GET, "Admin", "Supervisor")]

        public Task<IEnumerable<UserDtoV2>> GetSalesPersonsBySupervisor(Guid id)
        {
            return _commandBus.SendAsync(new GetSalesPersonsBySupervisorV2Query { Id = id});

        }
        [Route("/api/users/supervisors")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.GET,  "Admin", "SuperAdmin")]

        public  Task<IEnumerable<UserDtoV1>> GetSupervisors()
        {
            return _commandBus.SendAsync(new GetSupervisorsQuery {IncludeRoles = true});

        }
        [Route("/api/users/controllers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.GET, "Admin", "SuperAdmin")]

        public Task<IEnumerable<UserDtoV1>> GetVerifiers()
        {
            return _commandBus.SendAsync(new GetVerifiersQuery { IncludeRoles = true });

        }
        [Route("/api/users/executers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.GET, "Admin", "SuperAdmin")]

        public Task<IEnumerable<UserDtoV1>> GetExecuters()
        {
            return _commandBus.SendAsync(new GetExecutersQuery { IncludeRoles = true });

        }
        [Route("/api/users/consolidators")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.GET, "Admin", "SuperAdmin")]

        public Task<IEnumerable<UserDtoV1>> GetConsolidators()
        {
            return _commandBus.SendAsync(new GetConsolidatorsQuery { IncludeRoles = true });

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.POST, "SuperAdmin", "Admin")]

        public async Task<ActionResult<User>> Post([FromBody] UserModel model)
        {
            User user = new User
            {
                UserName = model.UserName,
                NormalizedUserName = model.UserName.ToUpper(),
                Email = model.Email,
                NormalizedEmail = model.Email.ToUpper(),
                EmailConfirmed = model.EmailConfirmed,
                PhoneNumber = model.PhoneNumber,
                PhoneNumberConfirmed = model.PhoneNumberConfirmed,
                TwoFactorEnabled = model.TwoFactorEnabled,
                LockoutEnabled = model.LockoutEnabled,
                LockoutEnd = model.LockoutEnd,
                AccessFailedCount = model.AccessFailedCount,
                OrganizationId = model.OrganizationId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                ManagerId = model.ManagerId

            };
            // save the user
            var result = string.IsNullOrEmpty(model.Password) ? await _userManager.CreateAsync(user) : await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return ApiCustomResponse(result);
            // save the user's claims
            foreach (var modelClaim in model.Claims)
                await SendAddUserClaimCommand(user.Id, modelClaim);
            // save the user's roles
            foreach (var userRole in model.UserRoles)
                await SendUserRoleCommand(user.Id, userRole);
            return Created($"/api/users/{model.Id}", model);

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)] 
        [Route("/api/users/e-commerce-registration")]
        public async Task<ActionResult<User>> Register([FromBody] RegistrationModel model)
        {
            User user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                NormalizedUserName = model.UserName.ToUpper(),
                Email = model.Email,
                NormalizedEmail = model.Email.ToUpper(),
                EmailConfirmed = model.EmailConfirmed,
                PhoneNumber = model.PhoneNumber,
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnabled = false
            };
           
            // save the user
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var _errors = new List<string>();
                foreach (var error in result.Errors)
                    _errors.Add(error.Description);

                return BadRequest(_errors);
            }
            await _commandBus.Send(new AddUserAddressCommand
            {
                Id = user.Id,
                Address = new Address
                {
                    Billing = true, City = model.City, Country = model.Country, State = model.State, Main = true,
                    Shipping = true, Street = model.Street
                }
            });

            await _commandBus.Send(new AddUserRoleCommandV1
            {
                Id = user.Id,
               RoleName = "OnlineCustomer"
            });
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            byte[] tokenGeneratedBytes = Encoding.UTF8.GetBytes(token);
            var codeEncoded = WebEncoders.Base64UrlEncode(tokenGeneratedBytes);
            var serverUrl = _configuration.GetValue<string>("IdentityServerAuthentication:Authority");
            var callBackUrl =   $"{serverUrl}Account/ConfirmEmailAddress?token={codeEncoded}&userId={user.Id}";
            var template =  EmailTemplateHelper.GetEmailConfirmationTemplate(new AccountModel {Name = user.FirstName + " " + user.LastName, Url = HtmlEncoder.Default.Encode(callBackUrl) });
            await _sender.SendEmailAsync(user.Email, "Email confirmation", template);
            return Created($"/api/users/{user.Id}", model);

        }
        private async Task SendUserRoleCommand(Guid userId, string userRole)
        {
            await _commandBus.Send(new AddUserRoleCommand
            {
                Id = userId,
                Role = new UserRole
                {
                    RoleId = Guid.Parse(userRole)
                },
            });
        }

        private async Task SendAddUserClaimCommand(Guid userId, ClaimModel modelClaim)
        {
            await _commandBus.Send(new AddClaimCommand
            {
                Id = userId,
                Claim = new UserClaim
                {
                    Type = modelClaim.Type,
                    Value = modelClaim.Value,
                },
            });
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.PUT, "SuperAdmin", "Admin")]

        public async Task<ActionResult> Put(Guid id, [FromBody] UserModel model)
        {
            User user = await _commandBus.SendAsync(new GetUserQuery {Id = id});
            user.Id = id;
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
            user.OrganizationId = model.OrganizationId;
            user.ManagerId = model.ManagerId;
           foreach (var modelClaim in model.Claims)
                 await SendAddUserClaimCommand(user.Id, modelClaim);
             // save the user's roles
             foreach (var userRole in model.UserRoles)
             {
                 await SendUserRoleCommand(user.Id, userRole);
             }
             var r = await _commandBus.SendAsync(new UpdateUserCommand {User = user});
            model = _mapper.Map<UserModel>(user);
            return Ok(model);
        }

        [HttpPut("{id}/password")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.PUT, "SuperAdmin", "Admin")]

        public async Task<ActionResult> SetPassword(Guid id, [FromBody] UserModel model)
        {
            User user = await _commandBus.SendAsync(new GetUserQuery { Id = id });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var rs = await _userManager.ResetPasswordAsync(user, token, model.Password);

            if (rs.Succeeded)
            {
                return Ok();
            }

            return BadRequest(rs.Errors);
        }
        [HttpPut("{id}/reset-password")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.PUT, "SuperAdmin", "Admin")]

        public async Task<ActionResult> ResetPassword(Guid id, [FromBody] UserModel model)
        {
            User user = await _commandBus.SendAsync(new GetUserQuery { Id = id });
            var check = await _userManager.CheckPasswordAsync(user, model.OldPassword);
            if (check)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var rs = await _userManager.ResetPasswordAsync(user, token, model.Password);

                if (rs.Succeeded)
                {
                    return Ok();
                }
                return BadRequest(rs.Errors);

            }

            return BadRequest("Impossible de changer le mot de passe, l'ancien mot de passe est incorrecte");
        }
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.DELETE, "SuperAdmin", "Admin")]

        public async Task<ActionResult> Delete(Guid id)
        {

            await _commandBus.Send(new DeleteUserCommand { Id = id });

            return Ok();
        }

        [HttpPost("{id}/passwordresetemail")]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.POST, "SuperAdmin", "Admin")]

        public async Task<ActionResult> SendResetPasswordEmail(Guid id)
        {
            var user = await _commandBus.SendAsync(new GetUserQuery { Id = id });

            if (user == null) return BadRequest();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var serverUrl = _configuration.GetValue<string>("IdentityServerAuthentication:Authority");
            var resetUrl = $"{serverUrl}Account/ResetPassword?token={HttpUtility.UrlEncode(token)}&email={user.Email}";
            var template = $"<a href='{resetUrl}'>Reset Url</a>";
            await _sender.SendEmailAsync(user.Email, "Reset password", template);
            return Ok();
        }

        [HttpPut("{id}/AssignToOrganization")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //  [ResourceAuthorization(PermissionItem.Membership, PermissionAction.Update, "SuperAdmin", "Admin")]

        public async Task<ActionResult> AssignToOrganization(Guid id, [FromBody] OrganizationModel model)
        {
            User user = await _commandBus.SendAsync(new GetUserQuery {Id = id});
            user.OrganizationId = model.Id;
            _ = await _userManager.UpdateAsync(user);
            return Ok(model);
        }

        [HttpPost("signin")]
        public async Task<IActionResult> Signin([FromBody] LoginViewModel model)
        {

            var client = new HttpClient();
            var url = _configuration.GetValue<string>("IdentityServerAuthentication:Authority");
            var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {

                Address = $"{url}connect/token",
                ClientId = "GHPCommerce.Mobile",
                ClientSecret = "secret",
                Scope = "openid profile GHPCommerce.WebApi",
                UserName = model.UserName,
                Password = model.Password
            });
            if (response.IsError)
            {
                return BadRequest(response.Error);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userManager.FindByNameAsync(model.UserName);
            if (result != null && await _userManager.CheckPasswordAsync(result, model.Password))
            {
                var profileViewModel = new ProfileViewModel(result, response);
                var customer = await _commandBus.SendAsync(new GetCustomerByOrganizationIdQuery {OrganizationId = result.OrganizationId.Value});
                if(customer==null)
                {
                    return Ok(profileViewModel);
                }
                var supplier = await _commandBus.SendAsync(new GetByIdOfSupplierQuery {Id = customer.SupplierId});
                if(supplier==null) return Ok(profileViewModel);

                profileViewModel.SupplierOrganizationId = supplier.OrganizationId;
                profileViewModel.SalesPersonId = customer.SalesPersonId??null;
                profileViewModel.CustomerId = customer.Id;
                return Ok(profileViewModel);
            }

            return BadRequest("Invalid username or password.");
        }

    }
}
