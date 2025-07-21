using AutoMapper;
using RealEstate.Domain;
using RealEstate.DTOs.CaseContact;
using RealEstate.DTOs.ListingCase;
using RealEstate.DTOs.MediaAsset;
using RealEstate.DTOs.Order;
using RealEstate.DTOs.User;



namespace RealEstate
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<ListingCaseCreateRequest, ListingCase>();
            CreateMap<ListingCase, ListingCaseResponseDto>();
            CreateMap<MediaAsset, MediaAssetResponseDto>();
             // Order mappings
            CreateMap<OrderCreateRequest, Order>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
              
            CreateMap<Order, OrderResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<User, FindAgentByEmailResponseDto>()
                .ForMember(dest => dest.AgentFirstName, opt => opt.MapFrom(src => src.AgentProfile.AgentFirstName))
                .ForMember(dest => dest.AgentLastName, opt => opt.MapFrom(src => src.AgentProfile.AgentLastName))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.AgentProfile.CompanyName));

            CreateMap<User, UserLoginInfoDto>();

            CreateMap<ListingCase, ListingWithStatusHistoryDto>();       
            CreateMap<ListingCaseUpdateRequestDto, ListingCase>();
            CreateMap<ListingCase, ListingCaseUpdateResponseDto>();

            CreateMap<PhotographyCompanyRegisterDto, User>();
            CreateMap<UserRegisterRequestDto, User>();
            CreateMap<User, UserRegisterResponseDto>();

            CreateMap<CaseContactRequestDto, CaseContact>();
            CreateMap<CaseContact, CaseContactResponseDto>();

            CreateMap<CreateAgentRequestDto, User>();
            CreateMap<User, CreateAgentResponseDto>();

            CreateMap<Agent, AgentResponseDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.AgentFirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.AgentLastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber));
            CreateMap<Agent, FindAgentByEmailResponseDto>()
                .ForMember(dest => dest.AgentFirstName, opt => opt.MapFrom(src => src.AgentFirstName))
                .ForMember(dest => dest.AgentLastName, opt => opt.MapFrom(src => src.AgentLastName))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber));
            
            CreateMap<PhotographyCompany, PhotographyCompanyResponseDto>()
              .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
              .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber));

            CreateMap<User, UserLoginInfoDto>()
                .ForMember(dest => dest.Role, opt => opt.Ignore());

            CreateMap<ListingCase, ListingCaseDetailResponseDto>()
               .ForMember(dest => dest.MediaAssets, opt => opt.Ignore());

            CreateMap<MediaAsset, MediaAssetDto>();

            CreateMap<CaseContact, CaseContactDto>()
                .ForMember(dest => dest.ContactId, opt => opt.MapFrom(src => src.ContactId));

            CreateMap<Agent, AgentDto>();
        }
    }
}
