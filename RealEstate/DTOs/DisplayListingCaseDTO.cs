namespace RealEstate.DTOs
{
    public class DisplayListingCaseDTO
    {
        public int Id { get; set; }
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
        public bool IsDeleted { get; set; }
        public string PropertyType { get; set; }
        public string SaleCategory { get; set; }
        public string ListcaseStatus { get; set; }

        //public List<MediaAssetDto> MediaAssets { get; set; } = new();
        //public List<AgentListingCaseDto> AgentListingCases { get; set; } = new();

        public string UserId { get; set; }
    }
}
