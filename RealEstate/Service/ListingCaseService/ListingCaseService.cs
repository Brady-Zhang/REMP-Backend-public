using AutoMapper;

using Microsoft.EntityFrameworkCore;

using Azure.Core;
using Microsoft.AspNetCore.Mvc;

using RealEstate.Collection;
using RealEstate.Data;
using RealEstate.Domain;
using RealEstate.DTOs.ListingCase;
using RealEstate.Enums;
using RealEstate.Exceptions;
using RealEstate.Repository.ListingCaseRepository;
using RealEstate.Repository.StatusHistoryRepository;
using System.Security.Claims;
using System.Transactions;
using RealEstate.Service.Email;

namespace RealEstate.Service.ListingCaseService
{
    public class ListingCaseService : IListingCaseService
    {
        private readonly IListingCaseRepository _listingCaseRepository;
        private readonly IMapper _mapper;
        private readonly IMongoDbContext _mongoDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<ListingCaseService> _logger;
        private readonly IStatusHistoryRepository _statusHistoryRepository;
        private readonly IEmailAdvancedSender _smtpEmailSender;
        private readonly IConfiguration _configuration;


        public ListingCaseService(IListingCaseRepository listingCaseRepository,
                                  IMapper mapper,
                                  IMongoDbContext mongoDbContext,
                                  IHttpContextAccessor httpContextAccessor,
                                  ApplicationDbContext applicationDbContext,
                                  ILogger<ListingCaseService> logger,
                                  IStatusHistoryRepository statusHistoryRepository,
                                  IEmailAdvancedSender smtpEmailSender,
                                  IConfiguration configuration)
        {
            _listingCaseRepository = listingCaseRepository;
            _mapper = mapper;
            _mongoDbContext = mongoDbContext;
            _httpContextAccessor = httpContextAccessor;
            _applicationDbContext = applicationDbContext;
            _logger = logger;
            _statusHistoryRepository = statusHistoryRepository;
            _smtpEmailSender = smtpEmailSender;
            _configuration = configuration;
        }

        // Mongo Database transaction
        public async Task LogAllChangeInMongo(ListingCase listingCase,
                                              string userId,
                                              bool logCaseHistory = false,
                                              ChangeAction? changeAction = null,
                                              bool logStatusHistory = false,
                                              ListcaseStatus? oldStatus = null,
                                              ListcaseStatus? newStatus = null,
                                              bool logUserActivity = false,
                                              UserActivityType? userActivityType = null)
        {
            //using var session = await _mongoDbContext.Client.StartSessionAsync();
            //session.StartTransaction();
            // Set session to null because current MongoDb is standalone.
            try
            {
                if (logCaseHistory == true && changeAction != null)
                {
                    try
                    {
                        await _listingCaseRepository.LogCaseHistoryAsync(null, listingCase, userId, changeAction.Value);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "Failed to insert CaseHistory");
                    }
                }
                if (logStatusHistory == true && oldStatus != null && newStatus != null)
                {
                    try
                    {
                        await _listingCaseRepository.LogStatusHistoryAsync(null, listingCase, userId, oldStatus.Value, newStatus.Value);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "Failed to insert StatusHistory");
                    }
                }
                if (logUserActivity == true && userActivityType != null)
                {
                    try
                    {
                        await _listingCaseRepository.LogUserActivityAsync(null, listingCase, userId, userActivityType.Value);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "Failed to insert UserActivityLog");
                    }
                }
                //await session.CommitTransactionAsync();
            }
            catch (Exception exception)
            {
                //await session.AbortTransactionAsync();
                _logger.LogError($"MongoDb transaction failed... " + exception.Message);
                throw;
            }
        }

        public async Task<ListingCaseResponseDto> CreateListingCase(ListingCaseCreateRequest listingCaseCreateRequest)
        {

            var listingCase = _mapper.Map<ListingCase>(listingCaseCreateRequest);
            listingCase.ListcaseStatus = ListcaseStatus.Created;

            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) 
            {
                throw new InvalidOperationException("UserId not found"); 
            }
            listingCase.UserId = userId;
            // SQL Database transaction
            try
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {

                    await _listingCaseRepository.CreateListingCase(listingCase);
                    scope.Complete();
                }
                try
                {
                    await LogAllChangeInMongo(
                        listingCase,
                        userId,

                        logCaseHistory: true,
                        changeAction: ChangeAction.Created,

                        logStatusHistory: true,
                        newStatus: ListcaseStatus.Created,
                        logUserActivity: true,
                        userActivityType: UserActivityType.LoggedIn
                    );
                }
                catch (Exception logException)
                {
                    _logger.LogError(logException, "Mongo logging failed");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError("Failed to creating listing case." + exception.Message);
                throw;
            }
            return _mapper.Map<ListingCaseResponseDto>(listingCase);
        }

        public async Task UpdateStatus(int caseId, ListcaseStatus newStatus, string userId, string role)
        {
            var listing = await _listingCaseRepository.GetListingCaseById(caseId);
            if (listing == null)
                throw new NotFoundException("Listing not found");

            var currentStatus = listing.ListcaseStatus;

            var validTransitions = new Dictionary<ListcaseStatus, ListcaseStatus>
            {
                { ListcaseStatus.Created, ListcaseStatus.Pending },
                { ListcaseStatus.Pending, ListcaseStatus.Delivered },
            };
            if (!validTransitions.TryGetValue(currentStatus, out var expected) || expected != newStatus)
            {
                throw new InvalidOperationException($"Invalid status transition: {currentStatus} → {newStatus}");

            }
            if (role == "Agent")
            {
                throw new UnauthorizedAccessException("Agents cannot change the status.");
            }
            await _listingCaseRepository.UpdateStatus(listing, newStatus);
            var agents = await _listingCaseRepository.GetAgentsOfListingCaseAsync(caseId);
          
            var loginUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
            var html = $@"
                <h3>Listing Status Updated</h3>
                <p>Listing Case (ID: {caseId}) has been delivered to you.</p>
                <p><a href='{loginUrl}'>Click here to view your listing</a></p>";

            foreach (var agent in agents)
            {
               
                if (agent.User?.Email != null)
                {
                    try 
                    {
                        await _smtpEmailSender.SendEmailAsync(agent.User.Email, "Your ListingCase Status Has Changed", html);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send status update email to user {Email}", listing.User.Email);
                    }
                }
            }
           
            //log status change in MongoDb
            await _listingCaseRepository.LogStatusHistoryAsync(
                session: null, 
                listingCase: listing,
                userId: userId,
                oldStatus: currentStatus,
                newStatus: newStatus 
            );
            
        }


        public async Task<List<ListingWithStatusHistoryDto>> GetAllListingsAsync(string userId, string role)
        {
            try
            {
                var query = _listingCaseRepository.GetAllListingCasesQueryable(userId, role);
                var listings = await query
                    .OrderByDescending(q => q.CreatedAt)
                    .ToListAsync();

                var dto = _mapper.Map<List<ListingWithStatusHistoryDto>>(listings);
                
                return dto;
              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve listings with history for user {UserId}", userId);
                throw;
            }
        }

        public async Task<ListingCaseUpdateResponseDto> UpdateListingCase(int id, ListingCaseUpdateRequestDto listingCaseUpdateRequest)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                throw new InvalidOperationException("UserId not found");
            }

            var existcase = await _listingCaseRepository.GetByIdAsync(id);
            if (existcase == null)
            {
                throw new KeyNotFoundException($"ListingCase with ID {id} not found.");
            }

            if (listingCaseUpdateRequest.Title != null)
                existcase.Title = listingCaseUpdateRequest.Title;

            if (listingCaseUpdateRequest.Description != null)
                existcase.Description = listingCaseUpdateRequest.Description;

            if (listingCaseUpdateRequest.ShareableUrl != null)
                existcase.ShareableUrl = listingCaseUpdateRequest.ShareableUrl;

            if (listingCaseUpdateRequest.Street != null)
                existcase.Street = listingCaseUpdateRequest.Street;

            if (listingCaseUpdateRequest.City != null)
                existcase.City = listingCaseUpdateRequest.City;

            if (listingCaseUpdateRequest.State != null)
                existcase.State = listingCaseUpdateRequest.State;

            if (listingCaseUpdateRequest.Postcode != null)
                existcase.Postcode = listingCaseUpdateRequest.Postcode.Value;

            if (listingCaseUpdateRequest.Longitude != null)
                existcase.Longitude = listingCaseUpdateRequest.Longitude.Value;

            if (listingCaseUpdateRequest.Latitude != null)
                existcase.Latitude = listingCaseUpdateRequest.Latitude.Value;

            if (listingCaseUpdateRequest.Price != null)
                existcase.Price = listingCaseUpdateRequest.Price.Value;

            if (listingCaseUpdateRequest.Bedrooms != null)
                existcase.Bedrooms = listingCaseUpdateRequest.Bedrooms.Value;

            if (listingCaseUpdateRequest.Bathrooms != null)
                existcase.Bathrooms = listingCaseUpdateRequest.Bathrooms.Value;

            if (listingCaseUpdateRequest.Garages != null)
                existcase.Garages = listingCaseUpdateRequest.Garages.Value;

            if (listingCaseUpdateRequest.FloorArea != null)
                existcase.FloorArea = listingCaseUpdateRequest.FloorArea.Value;

            if (listingCaseUpdateRequest.PropertyType != null)
                existcase.PropertyType = listingCaseUpdateRequest.PropertyType;

            if (listingCaseUpdateRequest.SaleCategory != null)
                existcase.SaleCategory = listingCaseUpdateRequest.SaleCategory;


            try
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _listingCaseRepository.UpdateListingCase(id, existcase);
                    scope.Complete();
                }

                try
                {
                    await LogAllChangeInMongo(
                        existcase,
                        userId,
                        logCaseHistory: true,
                        changeAction: ChangeAction.Updated,
                        logUserActivity: true,
                        userActivityType: UserActivityType.LoggedIn
                    );
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "Mongo logging failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update listing case. " + ex.Message);
                throw;
            }

            return _mapper.Map<ListingCaseUpdateResponseDto>(existcase);
        }

        public async Task<ListingCaseDetailResponseDto?> GetListingCaseDetailAsync(int id)
        {
            var listing = await _listingCaseRepository.GetByIdAsync(id);
            if (listing == null)
            {
                _logger?.LogWarning("Listing case with ID {Id} was not found.", id);
                return null;
            }

            // Map the basic properties
            var dto = _mapper.Map<ListingCaseDetailResponseDto>(listing);

            // Initialize the CategorizedMediaAssetsDto
            dto.MediaAssets = new CategorizedMediaAssetsDto();

            // Manually map and categorize media assets
            if (listing.MediaAssets != null)
            {
                foreach (var mediaAsset in listing.MediaAssets)
                {
                    var mediaAssetDto = _mapper.Map<MediaAssetDto>(mediaAsset);

                    switch (mediaAsset.MediaType)
                    {
                        case MediaType.Picture:
                            dto.MediaAssets.Picture.Add(mediaAssetDto);
                            break;
                        case MediaType.Video:
                            dto.MediaAssets.Video.Add(mediaAssetDto);
                            break;
                        case MediaType.FloorPlan:
                            dto.MediaAssets.FloorPlan.Add(mediaAssetDto);
                            break;
                        case MediaType.VRTour:
                            dto.MediaAssets.VRTour.Add(mediaAssetDto);
                            break;
                    }
                }
            }

            // Map case contacts if they exist
            if (listing.CaseContacts != null)
            {
                dto.CaseContacts = _mapper.Map<List<CaseContactDto>>(listing.CaseContacts);
            }

            // Map agents
            if (listing.AgentListingCases != null)
            {
                dto.Agents = listing.AgentListingCases
                    .Select(alc => _mapper.Map<AgentDto>(alc.Agent))
                    .ToList();
            }

            return dto;
        }


        public async Task<bool> AddAgentToListingCaseAsync(AddAgentToListingCaseDto dto)
        {
            if (string.IsNullOrEmpty(dto.AgentId))
            {
                throw new ArgumentException("Agent ID is required");
            }

            bool exists = await _listingCaseRepository.AgentExistsInListingCaseAsync(dto.AgentId, dto.ListingCaseId);
            if (exists)
            {
                throw new InvalidOperationException("Agent is already assigned to this listing case");
            }

            var agentListingCase = new AgentListingCase
            {
                AgentId = dto.AgentId,
                ListingCaseId = dto.ListingCaseId
            };

            return await _listingCaseRepository.AddAgentToListingCaseAsync(agentListingCase);
        }

        public async Task<bool> RemoveAgentFromListingCaseAsync(RemoveAgentFromListingCaseDto dto)
        {
            if (string.IsNullOrEmpty(dto.AgentId))
            {
                throw new ArgumentException("Agent ID is required");
            }

            bool exists = await _listingCaseRepository.AgentExistsInListingCaseAsync(dto.AgentId, dto.ListingCaseId);
            if (!exists)
            {
                throw new InvalidOperationException("Agent is not assigned to this listing case");
            }

            return await _listingCaseRepository.RemoveAgentFromListingCaseAsync(dto.AgentId, dto.ListingCaseId);
        }

        public async Task<bool> DeleteListingCaseAsync(int listingCaseId)
        {
            var success = await _listingCaseRepository.DeleteListingCaseAsync(listingCaseId);
            return success;
        }

        public async Task<string> GenerateShareableLinkAsync(int listingCaseId)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                throw new InvalidOperationException("User not authenticated.");

            var listing = await _listingCaseRepository.GetByIdAsync(listingCaseId);
            if (listing == null)
                throw new KeyNotFoundException($"Listing case {listingCaseId} not found.");

            // Avoid regenerating if already exists
            if (!string.IsNullOrEmpty(listing.ShareableUrl))
                return listing.ShareableUrl;
 
            var baseUrl = _configuration["FrontendBaseUrl"] ?? "https://defaultfrontend.com/property/";
            var guidPart = Guid.NewGuid().ToString("N")[..8]; // only alphanumeric
            var uniqueUrl = $"{baseUrl}{listingCaseId}-{guidPart}";

            listing.ShareableUrl = uniqueUrl;

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _listingCaseRepository.UpdateListingCase(listingCaseId, listing);
                scope.Complete();
            }

            try
            {
                await LogAllChangeInMongo(
                    listing,
                    userId,
                    logCaseHistory: true,
                    changeAction: ChangeAction.Shared,
                    logUserActivity: true,
                    userActivityType: UserActivityType.SharedLink
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mongo logging failed during shareable link generation.");
            }

            return uniqueUrl;
        }
    }
}
