
﻿using MongoDB.Driver;
using RealEstate.Domain;


namespace RealEstate.Repository.MediaAssetRepository
{
    public interface IMediaAssetRepository
    {
        Task<List<MediaAsset>> GetMediaAssetsByListingIdAsync(int listingCaseId);
        Task<MediaAsset> DeleteMediaById(int id);
        Task LogMediaDeleteAsync(IClientSessionHandle? session, int mediaAssetId, string userId);    
        Task<List<MediaAsset>> GetMediaAssetsByAgentId(string agentId);       
        Task<List<MediaAsset>> SelectMediaByAgent(int listingCaseId, List<int> mediaAssetId);
        Task<List<int>> PreviousSelectByAgent(int listingCaseId, string userId);
        Task<List<MediaAsset>> UploadMediaToListingCaseId(List<MediaAsset> mediaAssets);
        Task<MediaAsset> GetMediaAssetById(int id);

    }

}
