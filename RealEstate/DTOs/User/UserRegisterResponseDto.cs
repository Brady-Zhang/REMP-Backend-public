namespace RealEstate.DTOs.User
{
    public class UserRegisterResponseDto
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
