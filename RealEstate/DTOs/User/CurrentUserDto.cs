namespace RealEstate.DTOs.User
{
    public class CurrentUserDto
    {
        public UserLoginInfoDto User { get; set; }
        public List<int> ListingCaseIds { get; set; } = new List<int>();
    }
}
