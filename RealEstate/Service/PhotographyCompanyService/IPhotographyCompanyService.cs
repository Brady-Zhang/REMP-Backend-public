using RealEstate.Domain;
using RealEstate.DTOs.User;

namespace RealEstate.Service.PhotographyCompanyService
{
    public interface IPhotographyCompanyService
    {
        Task<AgentPhotographyCompany> AddAgentToPhotographyCompany(string photographyCompanyId, string agentId);
        Task<IEnumerable<AgentResponseDto>> GetAgentsByCompanyAsync(string photographyCompanyId);
    }
}
