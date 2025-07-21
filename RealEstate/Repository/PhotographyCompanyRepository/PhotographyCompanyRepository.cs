using Microsoft.EntityFrameworkCore;
using RealEstate.Data;
using RealEstate.Domain;
using System;
using System.Linq;

namespace RealEstate.Repository.PhotographyCompanyRepository
{
    public class PhotographyCompanyRepository : IPhotographyCompanyRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<PhotographyCompany> _logger;
        public PhotographyCompanyRepository(ApplicationDbContext applicationDbContext,
                                            ILogger<PhotographyCompany> logger)
        {
            _applicationDbContext = applicationDbContext;
            _logger = logger;
        }

        public async Task<AgentPhotographyCompany> AddAgentToPhotographyCompany(string photographyCompanyId, string agentId)
        {
            try
            {
                var company = await _applicationDbContext.PhotographyCompanies.FirstAsync(p => p.Id.Equals(photographyCompanyId));
                var agent = await _applicationDbContext.Agents.FirstAsync(p => p.Id.Equals(agentId));

                if (company == null || agent == null)
                {
                    _logger.LogError("Agent or PhotographyCompany not found.");
                    throw new InvalidOperationException("Agent or PhotographyCompany not found.");

                }

                var isExists = _applicationDbContext.AgentPhotographyCompanies
                    .FirstOrDefault(p => p.CompanyId.Equals(company.Id) && p.AgentId.Equals(agentId));
                if (isExists != null)
                {
                    _logger.LogError("Agent already exists in this photography company.");
                    throw new InvalidOperationException("Agent already exists in this photography company.");
                }

                var agentPhotographyCompany = new AgentPhotographyCompany
                {
                    CompanyId = company.Id,
                    AgentId = agent.Id,
                    PhotographyCompany = company,
                    Agent = agent
                };
                await _applicationDbContext.AgentPhotographyCompanies.AddAsync(agentPhotographyCompany);
                await _applicationDbContext.SaveChangesAsync();

                return agentPhotographyCompany;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Agent>> GetAgentsByCompanyIdAsync(string photographyCompanyId)
        {
            return await _applicationDbContext.Agents
                .Include(a => a.User)
                .Include(a => a.AgentPhotographyCompanies)
                .Where(a => a.AgentPhotographyCompanies.Any(apc => apc.CompanyId == photographyCompanyId))
                .ToListAsync();
        }
    }
}
