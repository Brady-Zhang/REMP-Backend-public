using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Data;
using RealEstate.Domain;
using RealEstate.DTOs.User;
using RealEstate.Exceptions;
using RealEstate.Repository.UserRepository;
using RealEstate.Service.UserService;
using System.Security.Claims;

namespace RealEstate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger, IUserRepository userRepository, ApplicationDbContext applicationDbContext)
        {
            _userService = userService;
            _logger = logger;
        }


        [HttpPost("CreateAdmin")]
        public async Task<IActionResult> CreateAdmin([FromBody] UserRegisterRequestDto userRegisterRequestDto)
        {
            try
            {
                var created = await _userService.CreateAdminAccountAsync(userRegisterRequestDto,"admin");
               
                _logger.LogInformation("User registered successfully.");
                return Ok(new { message = "User registered successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest( new { message = ex.Message });
            }
            catch (NotImplementedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing the request and could not create the user in UserRegisterController.");
                return StatusCode(500, new { message = "User register failed" });
            }

        }

        [HttpPost("CreatePhotographyCompany")]
        public async Task<IActionResult> CreatePhotographyCompany([FromBody] PhotographyCompanyRegisterDto dto)
        {
            try
            {
                var created = await _userService.CreatePhotographyCompanyAccountAsync(dto, "PhotographyCompany");

                _logger.LogInformation("User registered successfully.");
                return Ok("User registered successfully");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest( new { message = ex.Message });
            }
            catch (NotImplementedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing the request and could not create the user in UserRegisterController.");
                return StatusCode(500, new { message = "User register failed" });
            }

        }


        [HttpPost("CreateAgentAccount")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAgent([FromForm] CreateAgentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if(request.AvatarImage != null && request.AvatarImage.Length == 0)
                return BadRequest(new { message = "Avatar image is empty." });

            try
            {
                var result = await _userService.CreateAgentAsync(request);
                return Ok(result);
            }
            catch (EmailAlreadyExistsException ex)
            {
                _logger.LogWarning(ex, "Attempted to create an agent with an existing email.");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create agent.");
                return StatusCode(500, new { message = "An error occurred while creating the agent." });
            }
        }

        [HttpPut("update-agent")]
        public async Task<IActionResult> UpdateAgent([FromForm] UpdateAgentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.AvatarImage != null && request.AvatarImage.Length == 0)
                return BadRequest(new { message = "Avatar image is empty." });

            try
            {
                var result = await _userService.UpdateAgentAsync(request);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Attempted to update a non-existent agent.");
                return NotFound(new { message = ex.Message });
            }
            catch (EmailAlreadyExistsException ex)
            {
                _logger.LogWarning(ex, "Attempted to update an agent with an email that already exists.");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update agent.");
                return StatusCode(500, new { message = "An error occurred while updating the agent." });
            }
        }

        [HttpDelete("delete-agent/{id}")]
        public async Task<IActionResult> DeleteAgent(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "Agent ID is required." });
            }

            try
            {
                await _userService.DeleteAgentAsync(id);
                return Ok(new { message = "Agent deleted successfully." });
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Attempted to delete a non-existent agent.");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete agent.");
                return StatusCode(500, new { message = "An error occurred while deleting the agent." });
            }
        }

        [HttpGet("FindAgentByEmail/{email}")]
        public async Task<IActionResult> FindUserByEmailAsync(string email)
        {         
            try
            {

                var result = await _userService.FindUserByEmailAsync(email);
                if (result == null)
                {
                    _logger.LogWarning($"Agent with email {email} not found");
                    return NotFound(new {message = $"Agent with email {email} not found" });
                }
                return Ok(result);
            }   
            catch(NotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch(Exception ex)
            {
                return StatusCode(500);
            }
            
        }

        [HttpGet("search-agent")]
        public async Task<ActionResult<IEnumerable<FindAgentByEmailResponseDto>>> SearchAgents([FromQuery] string searchTerm)
        {
            var agents = await _userService.SearchAgentsAsync(searchTerm);
            return Ok(agents);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _userService.LoginAsync(dto);
            if (result == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password"
                });
            }

            return Ok(new
            {
                message = "Login successful",
                data = result
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid or expired token."
                });
            }

            var dto = await _userService.GetCurrentUserAsync(userId);
            return Ok(dto);
        }

        [HttpGet("GetAllAgents")]
        public async Task<ActionResult<IEnumerable<AgentResponseDto>>> GetAllAgents()
        {
            var agents = await _userService.GetAllAgentsAsync();
            return Ok(agents);
        }

        [HttpGet("{id}/agent")]
        public async Task<ActionResult<AgentResponseDto>> GetAgentById(string id)
        {
            var agent = await _userService.GetAgentByIdAsync(id);

            if (agent == null)
            {
                return NotFound();
            }

            return Ok(agent);
        }

        [HttpGet("GetAllPhotographyCompany")]
        public async Task<ActionResult<IEnumerable<PhotographyCompanyResponseDto>>> GetAllPhotographyCompany()
        {
            var photographyCompanies = await _userService.GetAllPhotographyCompaniesAsync();
            return Ok(photographyCompanies);
        }

        [HttpGet("{id}/photographyCompany")]
        public async Task<ActionResult<PhotographyCompanyResponseDto>> GetById(string id)
        {
            var photographyCompany = await _userService.GetPhotographyCompanyByIdAsync(id);

            if (photographyCompany == null)
                return NotFound();

            return Ok(photographyCompany);
        }


        [HttpPost("update-password")]
        public async Task<ActionResult<PhotographyCompanyResponseDto>> UpdatePassword([FromBody] UpdatePasswordDto updatePasswordDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                
            {
                _logger.LogWarning("Invalid user or expired token.");
                return Unauthorized(new
                {
                    message = "Invalid or expired token."
                });
            }


            var result = await _userService.UpdatePasswordAsync(userId, updatePasswordDto);
            if (result.Succeeded)
            {
                _logger.LogInformation("Password updated successfully");
                return Created();
            }

            return BadRequest(new { message = result.Errors.ToList()[0].Description });
        }
    }
}
