
using Microsoft.AspNetCore.Mvc;
using RealEstate.Domain;
﻿using RealEstate.DTOs.MediaAsset;
using RealEstate.Enums;

namespace RealEstate.Service.MediaAssetService
{
    public interface IMediaAssetService
    {
        Task<List<MediaAssetResponseDto>> GetMediaAssetsByListingIdAsync(int listingCaseId);
      
        Task<MediaAssetResponseDto> DeleteMediaById(int id);

        Task<List<MediaAssetResponseDto>> GetMediaAssetsByAgentId (string agentId);

        Task<List<MediaAssetResponseDto>> SelectMediaByAgent(int listingCaseId,MediaSelectDto mediaSelectDto);

        Task<List<MediaAssetResponseDto>> UploadMediaToListingCaseId([FromForm] UploadMediaRequest request);

        Task<(Stream Content, string ContentType, string FileName)> DownloadMediaAssetById(int id);
    }
}
