namespace RealEstate.DTOs.User
{
    public class CreateAgentRequestDto
    {
        public string Email { get; set; }
        public string AgentFirstName { get; set; }
        public string AgentLastName { get; set; }
        public IFormFile? AvatarImage { get; set; }
        public string CompanyName { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
