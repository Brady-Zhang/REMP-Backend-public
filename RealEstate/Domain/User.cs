using Microsoft.AspNetCore.Identity;

namespace RealEstate.Domain
{
    public class User : IdentityUser
    {
        public User()
        {
           
        }

        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Agent? AgentProfile { get; set; }
        public PhotographyCompany? PhotographyCompany { get; set; }
        public ICollection<ListingCase> ListingCases { get; set; } = new List<ListingCase>();
        public ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();
       

    }
}
