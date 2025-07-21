using AutoMapper;
using RealEstate.Collection;
using RealEstate.Domain;
using RealEstate.Exceptions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using RealEstate.Data;
using RealEstate.Domain;
using RealEstate.DTOs.ListingCase;
using RealEstate.DTOs.MediaAsset;
using RealEstate.Enums;
using RealEstate.Repository.MediaAssetRepository;
using System.Security.Claims;
using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using RealEstate.Service.AzureBlobStorage;

namespace RealEstate.Service.MediaAssetService
{
    public class MediaAssetService : IMediaAssetService
    {
        private readonly IMediaAssetRepository _mediaAssetRepository;
        private readonly IMapper _mapper;
        private readonly IMongoDbContext _mongoDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<MediaAssetService> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;
        private readonly IAzureBlobStorageService _azureBlobStorageService;

        public MediaAssetService(
            IMediaAssetRepository mediaAssetRepository,
            IMapper mapper, 
            IMongoDbContext mongoDbContext, 
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext applicationDbContext,
            ILogger<MediaAssetService> logger,
            BlobServiceClient blobServiceClient,
            IConfiguration configuration,
            IAzureBlobStorageService azureBlobStorageService
            )
        {
            _mediaAssetRepository = mediaAssetRepository;
            _mapper = mapper;
            _mongoDbContext = mongoDbContext;
            _httpContextAccessor = httpContextAccessor;
            _applicationDbContext = applicationDbContext;
            _logger = logger;
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
            _azureBlobStorageService = azureBlobStorageService;
        }       

        public async Task<List<MediaAssetResponseDto>> GetMediaAssetsByListingIdAsync(int listingCaseId)
        {

            var user = _httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = user?.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                _logger.LogWarning("User not authenticated. userId or role is missing.");
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var listingExists = await _applicationDbContext.ListingCases
                    .AnyAsync(x => x.Id == listingCaseId);

            if (!listingExists)
            {
                _logger.LogWarning("Listing case {ListingId} does not exist.", listingCaseId);
                throw new KeyNotFoundException("Listing case does not exist.");
            }

            if (role != "Admin")
            {
                var isAssigned = await _applicationDbContext.AgentListingCases
                    .AnyAsync(x => x.Agent.Id == userId && x.ListingCaseId == listingCaseId);

                if (!isAssigned)
                    throw new UnauthorizedAccessException("You are not assigned to this listing.");
            }

            var mediaAssets = await _mediaAssetRepository.GetMediaAssetsByListingIdAsync(listingCaseId);
            var result = _mapper.Map<List<MediaAssetResponseDto>>(mediaAssets);

            return result;
        }



        // Mongo Database transaction
        public async Task LogDeleteChangeInMongo(MediaAsset mediaAsset,string userId, bool LogMediaDelete = false)
        {
            //using var session = await _mongoDbContext.Client.StartSessionAsync();
            //session.StartTransaction();
            // Set session to null because current MongoDb is standalone.
            try
            {
                if (LogMediaDelete == true)
                {
                    try
                    {
                        await _mediaAssetRepository.LogMediaDeleteAsync(null, mediaAsset.Id, userId);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "Failed to insert MediaDelection");
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


        public async Task<MediaAssetResponseDto> DeleteMediaById(int id)
        {
            MediaAsset media;
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                throw new InvalidOperationException("UserId not found");
            }
            // SQL Database transaction
            try
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {

                    media = await _mediaAssetRepository.DeleteMediaById(id);
                    //await _azureBlobStorageService.DeleteFileAsync(media.MediaUrl);

                    scope.Complete();
                }
                try
                {
                    await LogDeleteChangeInMongo(
                        media,
                        userId,
                        LogMediaDelete: true
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
            return _mapper.Map<MediaAssetResponseDto>(media);
        }

         public async Task<List<MediaAssetResponseDto>> SelectMediaByAgent(int listingCaseId, MediaSelectDto mediaSelectDto)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                throw new InvalidOperationException("UserId not found");
            }
            var previousSelectCount = await _mediaAssetRepository.PreviousSelectByAgent(listingCaseId, userId);
            var newSelection = mediaSelectDto.MediaAssetId.Except(previousSelectCount).ToList();

            if(previousSelectCount.Count + newSelection.Count > 10)
            {
                throw new MediaSelectLimitExceedException($"Failed to select more than 10 media assets for ListingCaseId {listingCaseId}");
            }

            var selectedMedia = await _mediaAssetRepository.SelectMediaByAgent(listingCaseId, mediaSelectDto.MediaAssetId);

            await _mongoDbContext.SelectEvents.InsertOneAsync(new SelectEvent
            {
                ListingCaseId = listingCaseId.ToString(),
                SelectMediaIds = mediaSelectDto.MediaAssetId,
                UpdatedBy = userId,
                UpdatedAt = DateTime.Now
            });

            return _mapper.Map<List<MediaAssetResponseDto>>(selectedMedia);
            
        }

        public async Task<List<MediaAssetResponseDto>> GetMediaAssetsByAgentId (string agentId)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                throw new InvalidOperationException("UserId not found");
            }

            var role = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                _logger.LogWarning("User not authenticated. userId or role is missing.");
                throw new UnauthorizedAccessException("User is not authenticated.");
            }
            //if (userId != agentId)
            if (role == "Agent" && userId != agentId)
            {
                _logger.LogWarning("Agent tried to access another agent's media.");
                throw new UnauthorizedAccessException("Access denied.");
            }
            var mediaAssets = await _mediaAssetRepository.GetMediaAssetsByAgentId(agentId);
            return _mapper.Map<List<MediaAssetResponseDto>>(mediaAssets);
        }


        public async Task<List<MediaAssetResponseDto>> UploadMediaToListingCaseId([FromForm] UploadMediaRequest request)
        {
            var files = request.Files;
            var type = request.Type;
            var listingCaseId = request.ListingCaseId;
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                throw new InvalidOperationException("UserId not found");
            }
            try
            {           
                if (type != MediaType.Picture && files.Count > 1)
                {
                    throw new InvalidOperationException($"Only one file allowed for media type '{type}', but received {files.Count}.");
                }
                var uploadMediaUrl = await _azureBlobStorageService.UploadFilesAsync(files, $"MediaAssets/ListingCase-{listingCaseId}");
                if (uploadMediaUrl == null || !uploadMediaUrl.Any())
                {
                    throw new InvalidOperationException("Blob upload failed or returned empty URLs.");
                }
                var mediaAssets = uploadMediaUrl.Select(url => new MediaAsset
                {
                    ListingCaseId = listingCaseId,
                    MediaType = type,
                    MediaUrl = url,
                    UploadedAt = DateTime.Now,
                    UserId = userId
                }).ToList();
                await _mediaAssetRepository.UploadMediaToListingCaseId(mediaAssets);
                return _mapper.Map<List<MediaAssetResponseDto>>(mediaAssets);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Failed to upload media assets...");
                return null;
            }
        }

        public async Task<(Stream Content, string ContentType, string FileName)> DownloadMediaAssetById(int id)
        {
            try
            {
                var mediaAsset = await _mediaAssetRepository.GetMediaAssetById(id);
                if(mediaAsset == null)
                {
                    throw new FileNotFoundException($"Media Asset {id} not found...");
                }
                return await _azureBlobStorageService.DownloadFileAsync(mediaAsset.MediaUrl);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Failed to download media assets...");
                throw;
            }
        }

    }
}



