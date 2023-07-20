using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ICollection<string> _errors = new List<string>();
        protected ActionResult ApiCustomResponse()
        {
            if (!_errors.Any())
                return Ok();
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                {"ErrorMessages", _errors.ToArray()}
            }));
        }

        protected ActionResult ApiCustomResponse(ValidationResult validationResult)
        {
            if (validationResult?.Errors != null)
                foreach (var error in validationResult.Errors)
                    _errors.Add(error.ErrorMessage);

            return ApiCustomResponse();
        }
        protected ActionResult ApiCustomResponse(IdentityResult validationResult)
        {
            if (validationResult?.Errors != null)
                foreach (var error in validationResult.Errors)
                    _errors.Add(error.Description);

            return ApiCustomResponse();
        }
       
    }
}
