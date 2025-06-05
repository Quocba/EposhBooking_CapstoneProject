using AutoMapper;
using GraduationAPI_EPOSHBOOKING.DTO;
using GraduationAPI_EPOSHBOOKING.Model;
using Profile = AutoMapper.Profile;

namespace GraduationAPI_EPOSHBOOKING.Mapper
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper() { 
            CreateMap<Hotel, GetHotelDTO>().ReverseMap();
            CreateMap<RegisterDTO, Account>()
    .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true)) // Example mapping
    .ForMember(dest => dest.Role, opt => opt.Ignore()) // Ignore Role if not mapped directly
    .ForMember(dest => dest.Profile, opt => opt.Ignore()) // Ignore Profile if not mapped directly
    .ForMember(dest => dest.Hotel, opt => opt.Ignore()) // Ignore collections if not mapped directly
    .ForMember(dest => dest.MyVouchers, opt => opt.Ignore())
    .ForMember(dest => dest.Blogs, opt => opt.Ignore());
        }
    }
}
