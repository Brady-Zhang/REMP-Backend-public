namespace RealEstate.Domain
{
    //Agent - PhotographyCompany joining table
    public class AgentPhotographyCompany
    {
        public string AgentId { get; set; }
        public Agent Agent { get; set; }

        public string CompanyId { get; set; }
        public PhotographyCompany PhotographyCompany { get; set; }
    }
}
