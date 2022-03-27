using AutoMapper;
using IdentityAPI.Models;
using IdentityAPI.Models.DTO;

namespace IdentityAPI.Profiles
{
    public class ProfileMapper : Profile
    {
        public ProfileMapper()
        {
            CreateMap<UserRegistrationModel, User>();
       
        }
    }
}
