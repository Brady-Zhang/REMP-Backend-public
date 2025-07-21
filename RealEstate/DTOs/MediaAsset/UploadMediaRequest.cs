using RealEstate.Enums;

namespace RealEstate.DTOs.MediaAsset
{
    public class UploadMediaRequest
    {
        public int ListingCaseId { get; set; }
        public MediaType Type { get; set; }
        public List<IFormFile> Files { get; set; } = new List<IFormFile>();
    }
}
