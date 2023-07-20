using AutoMapper;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.WebApi.Models.Roles
{
    public class RoleModelMappingConfiguration : Profile
    {
        public RoleModelMappingConfiguration()
        {
            CreateMap<Role, RoleModel>();
        }
    }
}
