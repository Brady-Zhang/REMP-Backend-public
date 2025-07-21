using RealEstate.Domain;
using RealEstate.Enums;

namespace RealEstate.DTOs.MediaAsset
{
    public class MediaAssetResponseDto
    {
        public int Id { get; set; }
        public int ListingCaseId { get; set; }
        public string FileName { get; set; }
        public MediaType MediaType { get; set; }  //Picture,Video,FloorPlan,VRTour,
        public string? MediaUrl { get; set; } // Blob Url for file uploaded
        public DateTime UploadedAt { get; set; }
    }
}
