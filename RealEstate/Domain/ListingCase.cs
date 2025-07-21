using RealEstate.Enums;

namespace RealEstate.Domain
{
    public class ListingCase
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ShareableUrl { get; set; }
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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public PropertyType? PropertyType { get; set; } //House,Unit,Townhouse,Villa,Others
        public SaleCategory? SaleCategory { get; set; } //ForSale,ForRent,Auction
        public ListcaseStatus ListcaseStatus { get; set; } // Created,Pending,Delivered
        public ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();
        public ICollection<AgentListingCase> AgentListingCases { get; set; } = new List<AgentListingCase>();
        public ICollection<CaseContact> CaseContacts { get; set; } = new List<CaseContact>();
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
