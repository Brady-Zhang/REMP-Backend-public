namespace RealEstate.DTOs.User
{
    public class FindAgentByEmailResponseDto
    {    
        public string Id { get; set; }
        public string AgentFirstName { get; set; }
        public string AgentLastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CompanyName { get; set; }
    }
}
