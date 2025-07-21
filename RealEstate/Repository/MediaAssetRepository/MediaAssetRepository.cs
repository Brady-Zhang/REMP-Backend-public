using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using RealEstate.Collection;
using RealEstate.Data;
using RealEstate.Domain;
using RealEstate.Enums;

namespace RealEstate.Repository.MediaAssetRepository
{
    public class MediaAssetRepository : IMediaAssetRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IMongoDbContext _mongoDbContext;

        public MediaAssetRepository(ApplicationDbContext applicationDbContext, IMongoDbContext mongoDbContext)
        {
            _applicationDbContext = applicationDbContext;
            _mongoDbContext = mongoDbContext;

        }

        public async Task LogMediaDeleteAsync(IClientSessionHandle? session, int mediaAssetId, string userId)
        {
            var mediaDeletion = new MediaDeletion
            {
                Detail = $"MediaAsset with ID {mediaAssetId} is deleted.",
                DeleteBy = userId,
                DeleteAt = DateTime.Now
            };
            if (session != null)
            {
                await _mongoDbContext.MediaDeletions.InsertOneAsync(session, mediaDeletion);
            }
            else
            {
                await _mongoDbContext.MediaDeletions.InsertOneAsync(mediaDeletion);
            }

        }

        public async Task<MediaAsset> DeleteMediaById(int id)
        {
            var media = await _applicationDbContext.MediaAssets.FirstOrDefaultAsync(m => m.Id == id);

            if (media == null)
            {
                throw new InvalidOperationException($"The media is not existed or has been deleted already...");
            }
            _applicationDbContext.MediaAssets.Remove(media);
            await _applicationDbContext.SaveChangesAsync();
            return media;
        }

        public async Task<List<MediaAsset>> GetMediaAssetsByListingIdAsync(int listingCaseId)
        {
            return await _applicationDbContext.MediaAssets
                .Where(m => m.ListingCaseId == listingCaseId)
                .ToListAsync();
        }


        public async Task<List<MediaAsset>> GetMediaAssetsByAgentId(string agentId)
        {
            return await _applicationDbContext.MediaAssets.Where(m => m.UserId == agentId && !m.IsDeleted).ToListAsync();
        }


        public async Task<List<MediaAsset>> SelectMediaByAgent(int listingCaseId, List<int> mediaAssetId)
        {
            var mediaSelected = await _applicationDbContext.MediaAssets.Where(m =>
                    m.ListingCaseId == listingCaseId && mediaAssetId.Contains(m.Id)).ToListAsync();
            mediaSelected.ForEach(m => m.IsSelect = true);
            await _applicationDbContext.SaveChangesAsync();
            return mediaSelected;
        }
        public async Task<List<int>> PreviousSelectByAgent(int listingCaseId, string userId)
        {
            return await _applicationDbContext.MediaAssets.Where(m =>
                        m.ListingCaseId == listingCaseId &&
                        m.UserId == userId &&
                        m.IsSelect).Select(m => m.Id).ToListAsync();
        }

        public async Task<List<MediaAsset>> UploadMediaToListingCaseId(List<MediaAsset> mediaAssets)
        {
            foreach (var asset in mediaAssets) 
            {
                if (asset.MediaType == MediaType.Video || asset.MediaType == MediaType.FloorPlan || asset.MediaType == MediaType.VRTour)
                {
                    var existingAsset = await _applicationDbContext.MediaAssets
                        .Where(m => m.ListingCaseId == asset.ListingCaseId
                               && m.MediaType == asset.MediaType)
                        .FirstOrDefaultAsync();

                    if (existingAsset != null)
                    {
                        await DeleteMediaById(existingAsset.Id);
                    }
                }
            }
            await _applicationDbContext.MediaAssets.AddRangeAsync(mediaAssets);
            await _applicationDbContext.SaveChangesAsync();
            return mediaAssets;
        }

        public async Task<MediaAsset> GetMediaAssetById(int id)
        {
            return await _applicationDbContext.MediaAssets.FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}

