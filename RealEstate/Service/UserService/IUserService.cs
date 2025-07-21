
using Microsoft.AspNetCore.Identity;
using RealEstate.Domain;
using RealEstate.DTOs.User;


namespace RealEstate.Service.UserService
{
    public interface IUserService
    {
        Task<UserRegisterResponseDto> CreateAdminAccountAsync(UserRegisterRequestDto userRegisterRequestDto, string role);
        Task<UserRegisterResponseDto> CreatePhotographyCompanyAccountAsync(PhotographyCompanyRegisterDto userRegisterRequestDto, string role);
        Task<CreateAgentResponseDto> CreateAgentAsync(CreateAgentRequestDto createAgentRequestDto);
        Task<AgentResponseDto> UpdateAgentAsync(UpdateAgentRequestDto request);
        Task DeleteAgentAsync(string agentId);
        Task<FindAgentByEmailResponseDto> FindUserByEmailAsync(string email);
        Task<IEnumerable<FindAgentByEmailResponseDto>> SearchAgentsAsync(string searchTerm);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
        Task<CurrentUserDto> GetCurrentUserAsync(string userId);
        Task<IEnumerable<AgentResponseDto>> GetAllAgentsAsync();
        Task<AgentResponseDto> GetAgentByIdAsync(string id);
        Task<IEnumerable<PhotographyCompanyResponseDto>> GetAllPhotographyCompaniesAsync();
        Task<PhotographyCompanyResponseDto> GetPhotographyCompanyByIdAsync(string id);

        Task<IdentityResult> UpdatePasswordAsync(string userId, UpdatePasswordDto updatePasswordDto);
    }
}
