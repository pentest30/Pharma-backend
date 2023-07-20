using System;
using System.Threading.Tasks;

namespace GHPCommerce.Domain.Services
{
    public interface ICurrentOrganization
    {
      
        Task<Guid?> GetCurrentOrganizationIdAsync();
        Task<string> GetCurrentOrganizationNameAsync();
    }
}