using System;
using System.Security.Claims;
using GHPCommerce.Domain.Domain.Identity;
using Microsoft.AspNetCore.Http;

namespace GHPCommerce.Infra.Identity
{
    public class CurrentWebUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _context;

        public CurrentWebUser(IHttpContextAccessor context)
        {
            _context = context;
        }

        public bool IsAuthenticated => _context.HttpContext.User.Identity.IsAuthenticated;

        public Guid UserId
        {
            get
            {
                
                var userId = _context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? _context.HttpContext.User.FindFirst("sub")?.Value;

                return !string.IsNullOrEmpty(userId) ? Guid.Parse(userId) : Guid.Empty;
            }
            
        }
    }
}
