
using RealEstate.Enums;

namespace RealEstate.Domain
{
    public class MediaAsset
    {
        public int Id { get; set; }
        public MediaType MediaType { get; set; }  //Picture,Video,FloorPlan,VRTour,
        public string? MediaUrl { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public bool IsSelect { get; set; } = false;
        public bool IsHero { get; set; } = false;
        public int ListingCaseId { get; set; }
        public ListingCase ListingCase { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
