using RealEstate.Domain;

namespace RealEstate.Repository.PhotographyCompanyRepository
{
    public interface IPhotographyCompanyRepository
    {
        Task<AgentPhotographyCompany> AddAgentToPhotographyCompany(string photographyCompanyID, string agentID);
        Task<IEnumerable<Agent>> GetAgentsByCompanyIdAsync(string photographyCompanyId);

    }
}
