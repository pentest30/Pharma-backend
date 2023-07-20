using System.Threading.Tasks;

namespace GHPCommerce.Core.Shared.Services
{
    public interface ISAuthorizedIdsUser
    {
        Task<bool> IsAuthorized();
    }
}