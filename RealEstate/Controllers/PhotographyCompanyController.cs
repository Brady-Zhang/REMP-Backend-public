using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.DTOs.User;
using RealEstate.Service.PhotographyCompanyService;
using System.Security.Claims;

namespace RealEstate.Controllers
{
    [Route("api/PhotographyCompany")]
    [ApiController]
    public class PhotographyCompanyController : Controller
    {
        private readonly ILogger<PhotographyCompanyController> _logger;
        private readonly IPhotographyCompanyService _photographyCompanyService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PhotographyCompanyController(
            IPhotographyCompanyService photographyCompanyService,
            ILogger<PhotographyCompanyController> logger,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _photographyCompanyService = photographyCompanyService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        public async Task<ActionResult> AddAgentToPhotographyCompany([FromBody] string agentId)
        {
            var photographyCompanyId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (photographyCompanyId == null)
            {
                throw new InvalidOperationException("photographyCompanyId not found");
            }

            try
            {
                var result = await _photographyCompanyService.AddAgentToPhotographyCompany(photographyCompanyId, agentId);
                if (result == null)
                {
                    return NotFound("Agent or Photography Company not found Controller.");
                }
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding agent to photography company.");
                return StatusCode(500, "An error occurred while adding the agent to the photography company.");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AgentResponseDto>>> GetAgentsByCompany()
        {
            var photographyCompanyId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(photographyCompanyId))
            {
                return Unauthorized("User ID not found in token");
            }

            var agents = await _photographyCompanyService.GetAgentsByCompanyAsync(photographyCompanyId);
            return Ok(agents);
        }
    }
}
