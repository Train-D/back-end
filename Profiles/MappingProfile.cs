using AutoMapper;
using Google.Apis.Auth;
using Train_D.DTO;
using Train_D.DTO.UserDtos;
using Train_D.Models;

namespace Train_D.Profiles
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<Station, StationDTO>().ReverseMap();
            CreateMap<RegisterModel, User>().ReverseMap();
            CreateMap<RegisterModel, AuthModel>().ReverseMap();
            CreateMap<GoogleJsonWebSignature.Payload, User>()
                .ForMember(dest => dest.UserName, src => src.MapFrom(src => src.Email.Substring(0, src.Email.IndexOf("@"))))
                .ForMember(dest => dest.FirstName, src => src.MapFrom(src => src.GivenName))
                .ForMember(dest => dest.FirstName, src => src.MapFrom(src => src.GivenName));
           
        }
    }
}
