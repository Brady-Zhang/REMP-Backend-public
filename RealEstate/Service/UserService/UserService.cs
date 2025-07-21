using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using RealEstate.Collection;
using RealEstate.Controllers;
using RealEstate.Exceptions;
using Microsoft.AspNetCore.Identity;
using RealEstate.Constants;
using RealEstate.Domain;
using RealEstate.DTOs.User;
using RealEstate.Repository.UserRepository;
using RealEstate.Service.JwtService;
using RealEstate.Service.LoggerService;
using Microsoft.AspNetCore.Identity.UI.Services;
using RealEstate.Service.Email;
using Azure.Core;
using RealEstate.Service.AzureBlobStorage;
using Microsoft.EntityFrameworkCore;
using RealEstate.Data;

namespace RealEstate.Service.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly ILoggerService _loggerService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IEmailAdvancedSender _emailSender;
        private readonly IAzureBlobStorageService _blobService;
        private readonly ApplicationDbContext _context;
        public UserService(IUserRepository userRepository,
                                    IMapper mapper, 
                                    ILogger<UserService> logger,
                                    RoleManager<Role> roleManager,
                                    UserManager<User> userManager,
                                    ILoggerService loggerService,
                                    ITokenService tokenService,
                                    IConfiguration configuration,
                                    IEmailAdvancedSender emailSender,
                                    IAzureBlobStorageService blobService,
                                    ApplicationDbContext context)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
            _loggerService = loggerService;
            _tokenService = tokenService;
            _configuration = configuration;
            _emailSender = emailSender;
            _blobService = blobService;
            _context = context;
        }
        public async Task<CreateAgentResponseDto> CreateAgentAsync(CreateAgentRequestDto request)
        {
            try
            {
                await _userRepository.BeginTransactionAsync();
                //check email is already exist          
                var existingUser = await _userRepository.GetByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    throw new EmailAlreadyExistsException("An account with this email already exists.");
                }

                string? avatarUrl = null;
                if (request.AvatarImage != null)
                {
                    var file = new List<IFormFile> { request.AvatarImage }; 
                    var urls = await _blobService.UploadFilesAsync(file, "AgentAvatar");
                    avatarUrl = urls.FirstOrDefault();
                }

                var user = _mapper.Map<User>(request);
                user.UserName = request.Email;
                user.Email = request.Email;
                user.PhoneNumber = request.PhoneNumber;
                var password = GenerateRandomPassword();

                await _userRepository.UserRegisterRepositoryAsync(user, password);
                await _userRepository.UserRegisterAddRoleAsync(user, "Agent");

                var agent = new Agent
                {
                    Id = user.Id,
                    CompanyName = request.CompanyName,
                    AgentFirstName = request.AgentFirstName,
                    AgentLastName = request.AgentLastName,
                    AvatarUrl = avatarUrl,
                };

                await _userRepository.CreateAgentAsync(agent);

                var loginUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
                var html = $@"
                    <h3>Welcome to RealEstate!</h3>
                    <p>Your account has been created.</p>
                    <p>Email: {user.Email}</p>
                    <p>Password: {password}</p>
                    <p><a href='
                {loginUrl}'>Click here to login</a></p>";

                await _emailSender.SendEmailAsync(user.Email, "Your Agent Account", html);

                await _userRepository.CommitTransactionAsync();

                return new CreateAgentResponseDto
                {
                    Email = user.Email,
                    TempPassword = password
                };
            }
            catch (Exception ex)
            {
                await _userRepository.RollbackTransactionAsync();
                _logger.LogError(ex, "Failed to create agent account.");
                throw;
            }
        }

        public async Task<AgentResponseDto> UpdateAgentAsync(UpdateAgentRequestDto request)
        {
            try
            {
                await _userRepository.BeginTransactionAsync();

                var agent = await _userRepository.GetAgentByIdAsync(request.Id);
                if (agent == null)
                {
                    throw new NotFoundException($"Agent with ID {request.Id} not found.");
                }

                var user = await _userRepository.GetByIdAsync(agent.User.Id);
                if (user == null)
                {
                    throw new Exception($"User associated with agent {request.Id} not found.");
                }

                user.Email = request.Email;
                user.NormalizedEmail = request.Email.ToUpper();
                user.UserName = request.Email;
                user.NormalizedUserName = request.Email.ToUpper();
                user.PhoneNumber = request.PhoneNumber;
              
                agent.AgentFirstName = request.AgentFirstName;
                agent.AgentLastName = request.AgentLastName;
                agent.CompanyName = request.CompanyName;

                if (request.AvatarImage != null)
                {
                    var file = new List<IFormFile> { request.AvatarImage };
                    var urls = await _blobService.UploadFilesAsync(file, "AgentAvatar");
                    agent.AvatarUrl = urls.FirstOrDefault();
                }

                await _userRepository.UpdateAsync(user);
                await _userRepository.UpdateAgentAsync(agent);
                await _userRepository.CommitTransactionAsync();

                return _mapper.Map<AgentResponseDto>(agent);
            }
            catch (Exception ex)
            {
                await _userRepository.RollbackTransactionAsync();

                _logger.LogError(ex, "Error updating agent with ID {AgentId}", request.Id);
                throw;
            }
        }

        public async Task DeleteAgentAsync(string agentId)
        {
            try
            {
                await _userRepository.BeginTransactionAsync();

                var agent = await _userRepository.GetAgentByIdAsync(agentId);
                if (agent == null)
                {
                    throw new NotFoundException($"Agent with ID {agentId} not found.");
                }

                var user = await _userRepository.GetByIdAsync(agent.User.Id);
                if (user == null)
                {
                    throw new Exception($"User associated with agent {agentId} not found.");
                }

                await _userRepository.DeleteAgentAsync(agent);
                await _userRepository.DeleteAsync(user);
                await _userRepository.CommitTransactionAsync();

                _logger.LogInformation("Agent with ID {AgentId} has been deleted", agentId);
            }
            catch (Exception ex)
            {
                await _userRepository.RollbackTransactionAsync();

                _logger.LogError(ex, "Error deleting agent with ID {AgentId}", agentId);
                throw;
            }
        }

        public async Task<UserRegisterResponseDto> CreatePhotographyCompanyAccountAsync(PhotographyCompanyRegisterDto userRegisterRequestDto, string role)
        {
            try
            {
                await _userRepository.BeginTransactionAsync();

                var user = _mapper.Map<User>(userRegisterRequestDto);

                var isEmailExist = await _userRepository.IsEmailExist(userRegisterRequestDto.Email);
                if (isEmailExist)
                {
                    _logger.LogWarning("Email already registered");
                    throw new InvalidOperationException("Email already registered");
                }

                var checkRole = _roleManager.RoleExistsAsync(role);
                if (!checkRole.Result)
                {
                    _logger.LogWarning("Role does not exist.");
                    throw new InvalidOperationException("Role does not exist.");
                }

                var result = await _userRepository.UserRegisterRepositoryAsync(user, userRegisterRequestDto.Password);
                if (result == null)
                {
                    _logger.LogWarning("Failed to call create user with password method.");
                    throw new NotImplementedException("Failed to call create user with password method.");
                }
                var roleResult = await _userRepository.UserRegisterAddRoleAsync(user, role);
                if (roleResult == null)
                {
                    _logger.LogWarning("Failed to call add user role method.");
                    throw new NotImplementedException("Failed to call add user role method.");

                }

                var photographyCompany = new PhotographyCompany
                {
                    Id = result.Id,
                    PhotographyCompanyName = userRegisterRequestDto.PhotographyCompanyName,
                  
                };

                await _userRepository.CreatePhotographyCompanyAsync(photographyCompany);

                await _userRepository.CommitTransactionAsync();

                _logger.LogInformation("Run RegisterUserService successfully.");


                return _mapper.Map<UserRegisterResponseDto>(result);
            }
            catch (InvalidOperationException ex)
            {
                await _userRepository.RollbackTransactionAsync();
                _logger.LogError(ex, "Unexpected error while processing the register user in UserRegisterService.");
                throw;
            }
            catch (NotImplementedException ex)
            {
                await _userRepository.RollbackTransactionAsync();
                _logger.LogError(ex, "Unexpected error while processing the register user in UserRegisterService.");
                throw;
            }
            catch (Exception ex)
            {
                await _userRepository.RollbackTransactionAsync();
                _logger.LogError(ex, "Unexpected error while processing the register user in UserRegisterService.");
                throw;
            }

        }

        public async Task<UserRegisterResponseDto> CreateAdminAccountAsync(UserRegisterRequestDto userRegisterRequestDto, string role)
        {
           
            try
            {
                await _userRepository.BeginTransactionAsync();
                var user = _mapper.Map<User>(userRegisterRequestDto);

                var isEmailExist = await _userRepository.IsEmailExist(userRegisterRequestDto.Email);
                if (isEmailExist) 
                {
                    _logger.LogWarning("Email already registered");
                    throw new InvalidOperationException("Email already registered");
                }

                var checkRole = _roleManager.RoleExistsAsync(role);
                if (!checkRole.Result)
                {
                    _logger.LogWarning("Role does not exist.");
                    throw new InvalidOperationException("Role does not exist.");
                }

                var result = await _userRepository.UserRegisterRepositoryAsync(user, userRegisterRequestDto.Password);
                if (result == null)
                {
                    _logger.LogWarning("Failed to call create user with password method.");
                    throw new NotImplementedException("Failed to call create user with password method.");
                }
                var roleResult = await _userRepository.UserRegisterAddRoleAsync(user, role);
                if (roleResult == null)
                {
                    _logger.LogWarning("Failed to call add user role method.");
                    throw new NotImplementedException("Failed to call add user role method.");

                }
                await _userRepository.CommitTransactionAsync();
                _logger.LogInformation("Run RegisterUserService successfully.");

                return _mapper.Map<UserRegisterResponseDto>(result);
            }
            catch (Exception ex)
            {
                await _userRepository.RollbackTransactionAsync();

                _logger.LogError(ex, "Unexpected error while processing the register user in UserRegisterService.");
                throw;
            }

        }


        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                await _loggerService.LogUserActivityAsync(dto.Email, ErrorMessages.InvalidLogin);
                return null;
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
            {
                await _loggerService.LogUserActivityAsync(user.Id, ErrorMessages.InvalidLogin);
                return null;
            }

            var token = await _tokenService.GenerateTokenAsync(user);

            await _loggerService.LogUserActivityAsync(user.Id, "Login success");

            var userDto = _mapper.Map<UserLoginInfoDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();
            userDto.Role = role;

            if (role == "Agent")
            {
                var agent = await _context.Agents.FirstOrDefaultAsync(a => a.Id == user.Id);
                if (agent != null)
                {
                    userDto.AgentName = agent.AgentFirstName; 
                }
            }


            return new LoginResponseDto
            {
                Token = token,
                User = userDto,
                ListingCaseIds = user.ListingCases?.Select(l => l.Id).ToList() ?? new List<int>()
            };
        }

        public async Task<CurrentUserDto> GetCurrentUserAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException();
            }

            var userDto = _mapper.Map<UserLoginInfoDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();
            userDto.Role = role;

            await _loggerService.LogUserActivityAsync(userId, "Accessed/users/me");

            return new CurrentUserDto
            {
                User = userDto,
                ListingCaseIds = user.ListingCases?.Select(l => l.Id).ToList() ?? new List<int>()
            };
        }

        public async Task<FindAgentByEmailResponseDto> FindUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.FindUserByEmailAsync(email);

                if (user == null)
                    throw new NotFoundException($"User with email '{email}' was not found.");

                if (!await _userRepository.UserIsAgent(user.Id))
                    throw new NotFoundException($"User with email '{email}' is not an agent.");

                return _mapper.Map<FindAgentByEmailResponseDto>(user);  
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error occurred while finding agent by email: {email}");
                throw;
            }
        }

        public async Task<IEnumerable<FindAgentByEmailResponseDto>> SearchAgentsAsync(string searchTerm)
        {
            var agents = await _userRepository.SearchAgentsAsync(searchTerm);
            return _mapper.Map<IEnumerable<FindAgentByEmailResponseDto>>(agents);
           
        }

        public async Task<IEnumerable<AgentResponseDto>> GetAllAgentsAsync()
        {
            var agents = await _userRepository.GetAllAgentsAsync();
            return _mapper.Map<IEnumerable<AgentResponseDto>>(agents);
        }

        public async Task<AgentResponseDto> GetAgentByIdAsync(string id)
        {
            var agent = await _userRepository.GetAgentByIdAsync(id);
            return agent == null ? null : _mapper.Map<AgentResponseDto>(agent);
        }

        public async Task<IEnumerable<PhotographyCompanyResponseDto>> GetAllPhotographyCompaniesAsync()
        {
            var photographyCompanies = await _userRepository.GetAllPhotographyCompanyAsync();

            return _mapper.Map<IEnumerable<PhotographyCompanyResponseDto>>(photographyCompanies);
        }

        public async Task<PhotographyCompanyResponseDto> GetPhotographyCompanyByIdAsync(string id)
        {
            var photographyCompany = await _userRepository.GetPhotographyCompanyByIdAsync(id);
            return photographyCompany == null ? null : _mapper.Map<PhotographyCompanyResponseDto>(photographyCompany);
        }


        private string GenerateRandomPassword()
        {
            return Guid.NewGuid().ToString().Substring(0, 10) + "!";
        }

        public async Task<IdentityResult> UpdatePasswordAsync(string userId, UpdatePasswordDto updatePasswordDto)
        {

           
            var user = await _userRepository.GetByIdAsync(userId);
            if(user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            }
            else
            {
                var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, updatePasswordDto.currentPassword);
                if (isPasswordCorrect)
                {
                    return await _userRepository.UpdatePasswordAsync(user, updatePasswordDto.currentPassword, updatePasswordDto.newPassword);
                }
                else
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Wrong password" });
                }
            }
                
        }
    }
}

