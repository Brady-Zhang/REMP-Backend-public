using RealEstate.Enums;

namespace RealEstate.DTOs.ListingCase
{
    public class ListingCaseUpdateResponseDto
    {
        public string Title { get; set; }
        public string? ShareableUrl { get; set; }
        public PropertyType PropertyType { get; set; } //House,Unit,Townhouse,Villa,Others
        public SaleCategory SaleCategory { get; set; } //ForSale,ForRent,Auction
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public int? Postcode { get; set; }
        public double? Price { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public int? Garages { get; set; }
        public double? FloorArea { get; set; }
        public string UserId { get; set; }
    }
}
