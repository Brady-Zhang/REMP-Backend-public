namespace RealEstate.DTOs.CaseContact
{
    public class CaseContactRequestDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? CompanyName { get; set; }
        public string? ProfileUrl { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int ListingCaseId { get; set; }
    }
}
