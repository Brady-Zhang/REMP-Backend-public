namespace RealEstate.Domain
{
    public class Agent
    {
       
        public string Id { get; set; }
        public string? AgentFirstName { get; set; }
        public string? AgentLastName { get; set; }
        public string? AvatarUrl { get; set; } 
        public string? CompanyName { get; set; }

        public User User { get; set; }
        public ICollection<AgentListingCase> AgentListingCases { get; set; } = new List<AgentListingCase>();
        public ICollection<AgentPhotographyCompany> AgentPhotographyCompanies { get; set; } = new List<AgentPhotographyCompany>();

    }
}
