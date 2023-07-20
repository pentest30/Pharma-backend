using AutoMapper;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.WebApi.Models.Users
{
    public class UserModelMappingConfiguration : Profile
    {
        public UserModelMappingConfiguration()
        {
            CreateMap<User, UserModel>().ReverseMap();
        }

    }
}
