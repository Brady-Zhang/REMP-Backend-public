namespace RealEstate.DTOs.User
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public UserLoginInfoDto User { get; set; }
        public List<int> ListingCaseIds { get; set; } = new List<int>();
    }
}
