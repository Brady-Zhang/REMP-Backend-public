using RealEstate.Domain;
using RealEstate.DTOs.MediaAsset;
using RealEstate.Enums;

namespace RealEstate.DTOs.ListingCase
{
    public class ListingCaseDetailResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public int? Postcode { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
        public double? Price { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public int? Garages { get; set; }
        public double? FloorArea { get; set; }
        public DateTime CreatedAt { get; set; }
        public PropertyType PropertyType { get; set; }
        public SaleCategory SaleCategory { get; set; }
        public ListcaseStatus ListcaseStatus { get; set; }

        public CategorizedMediaAssetsDto MediaAssets { get; set; } = new CategorizedMediaAssetsDto();
        public ICollection<CaseContactDto> CaseContacts { get; set; } = new List<CaseContactDto>();
        public ICollection<AgentDto> Agents { get; set; } = new List<AgentDto>();
    }

    public class MediaAssetDto
    {
        public int Id { get; set; }
        public MediaType MediaType { get; set; }
        public string? MediaUrl { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool IsSelect { get; set; }
        public bool IsHero { get; set; }
    }

    public class AgentDto
    {
        public string Id { get; set; }
        public string? AgentFirstName { get; set; }
        public string? AgentLastName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CompanyName { get; set; }
    }

    public class CaseContactDto
    {
        public int ContactId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string ProfileUrl { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }


    public class CategorizedMediaAssetsDto
    {
        public ICollection<MediaAssetDto> Picture { get; set; } = new List<MediaAssetDto>();
        public ICollection<MediaAssetDto> Video { get; set; } = new List<MediaAssetDto>();
        public ICollection<MediaAssetDto> FloorPlan { get; set; } = new List<MediaAssetDto>();
        public ICollection<MediaAssetDto> VRTour { get; set; } = new List<MediaAssetDto>();
    }
}