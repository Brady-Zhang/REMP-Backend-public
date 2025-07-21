using AutoMapper;
using Microsoft.Extensions.Logging;
using RealEstate.Data;
using RealEstate.Domain;
using RealEstate.DTOs.User;
using RealEstate.Repository.ListingCaseRepository;
using RealEstate.Repository.PhotographyCompanyRepository;
using RealEstate.Repository.StatusHistoryRepository;

namespace RealEstate.Service.PhotographyCompanyService
{
    public class PhotographyCompanyService : IPhotographyCompanyService
    {
        private readonly IPhotographyCompanyRepository _photographyCompanyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PhotographyCompanyService> _logger;

        public PhotographyCompanyService(IPhotographyCompanyRepository photographyCompanyRepository, IMapper mapper, ILogger<PhotographyCompanyService> logger)
        {
            _photographyCompanyRepository = photographyCompanyRepository;
            _mapper = mapper;
            _logger = logger;
        }
        public Task<AgentPhotographyCompany> AddAgentToPhotographyCompany(string PhotographyCompanyId,string AgentId)
        {
            try
            {
                var photographyCompanyId = PhotographyCompanyId;
                var agentId =AgentId;
                var result = _photographyCompanyRepository.AddAgentToPhotographyCompany(photographyCompanyId, agentId);
                if (result == null)
                {
                    _logger.LogError("Failed to add agent to photography company.");
                    throw new InvalidOperationException("Failed to add agent to photography company.");
                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<AgentResponseDto>> GetAgentsByCompanyAsync(string photographyCompanyId)
        {
            var agents = await _photographyCompanyRepository.GetAgentsByCompanyIdAsync(photographyCompanyId);

            return _mapper.Map<IEnumerable<AgentResponseDto>>(agents);
        }
    }
}
