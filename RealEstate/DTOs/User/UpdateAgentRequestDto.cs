using System.ComponentModel.DataAnnotations;

namespace RealEstate.DTOs.User
{
    public class UpdateAgentRequestDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string AgentFirstName { get; set; }
        public string AgentLastName { get; set; }
        public IFormFile? AvatarImage { get; set; }
        public string CompanyName { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
