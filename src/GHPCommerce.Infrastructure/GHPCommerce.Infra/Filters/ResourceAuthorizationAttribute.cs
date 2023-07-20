#nullable enable
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GHPCommerce.Infra.Filters
{
    public  class ResourceAuthorizationAttribute : TypeFilterAttribute
    {
        public ResourceAuthorizationAttribute(PermissionItem item, PermissionAction action, params string?[] roles ) : base(typeof(ClaimRequirementFilter))
        {
            Arguments = new object[] { item, action, roles };
        }

        private class ClaimRequirementFilter : IAuthorizationFilter
        {
            private readonly PermissionItem _item;
            private readonly PermissionAction _action;
            private readonly string[]? _roles;
            public ClaimRequirementFilter( PermissionItem item, PermissionAction action, string [] roles)
            {
                _item = item;
                _action = action;
                _roles = roles;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {

                var currentUser = context.HttpContext.User;
                // parcourir les permissions d'utilisateur
                var hasClaim = currentUser.Claims.Any(c => c.Type.ToLower() == _item.ToString().ToLower() && c.Value.ToLower() == _action.ToString().ToLower());
                // si l'utilisateur possède les permissions requises pour la ressource demandée on sort  
                if (hasClaim) return;
                // parcourir les roles d'utilisateur
                if (_roles != null && _roles.Any())
                {
                    //    si l'utilisateur possède les roles requis pour la ressource demandée on sort  
                    if (_roles.Any(role => currentUser.IsInRole(role)))
                        return;

                }
                // on retourne accès non autorisé

                context.Result = new ForbidResult();
            }
        }
    }
}
