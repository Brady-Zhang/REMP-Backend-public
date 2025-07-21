using System.ComponentModel.DataAnnotations;

namespace RealEstate.Domain
{
    public class PhotographyCompany
    {
        public string Id { get; set; }
        public string PhotographyCompanyName { get; set; }
        public User User { get; set; }
        public ICollection<AgentListingCase> AgentListingCases { get; set; } = new List<AgentListingCase>();
        public ICollection<AgentPhotographyCompany> AgentPhotographyCompanies { get; set; } = new List<AgentPhotographyCompany>();

    }
}
    