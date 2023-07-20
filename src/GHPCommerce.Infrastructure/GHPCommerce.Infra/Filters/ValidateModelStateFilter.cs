using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GHPCommerce.Infra.Filters
{
    public class ValidateModelStateFilter: IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            //throw new System.NotImplementedException();
        }

        public  void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
                return;

            var validationErrors = context.ModelState
                .Keys
                .SelectMany(k => context.ModelState[k].Errors)
                // ReSharper disable once TooManyChainedReferences
                .Select(e => e.ErrorMessage);
               

            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                {"ErrorMessages", validationErrors.ToArray()}
            }));
        }
    }
}
