using Azure;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstate.Domain;
using System.Reflection.Metadata;
using System.Security.Claims;

namespace RealEstate.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, string>
    {
        public DbSet<Agent> Agents { get; set; }
        public DbSet<ListingCase> ListingCases { get; set; }
        public DbSet<MediaAsset> MediaAssets { get; set; }
        public DbSet<AgentListingCase> AgentListingCases { get; set; }
        public DbSet<Order> Orders { get; set; }
        
        public DbSet<CaseContact> CaseContacts { get; set; }
        
        public DbSet<PhotographyCompany> PhotographyCompanies { get; set; }
        public DbSet<AgentPhotographyCompany> AgentPhotographyCompanies { get; set; }



        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region User Table
            modelBuilder.Entity<User>()
                .HasMany(u => u.MediaAssets)
                .WithOne(ma => ma.User) 
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>()
                .HasMany(u => u.ListingCases)
                .WithOne(la => la.User)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion


            #region Listcase Table
            modelBuilder.Entity<ListingCase>(entity =>
            {
                entity.Property(e => e.Latitude)
                      .HasPrecision(12, 9); 

                entity.Property(e => e.Longitude) 
                      .HasPrecision(12, 9);

                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(255);
            });
            #endregion


            #region Agent Table
            modelBuilder.Entity<Agent>()
                .HasOne(a => a.User)
                .WithOne(u => u.AgentProfile)
                .HasForeignKey<Agent>(a => a.Id)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion


            #region Agent - ListingCase joint table
            modelBuilder.Entity<AgentListingCase>()
                .HasKey(al => new { al.AgentId, al.ListingCaseId });

            modelBuilder.Entity<AgentListingCase>()
                .HasOne(al => al.Agent)
                .WithMany(a => a.AgentListingCases)
                .HasForeignKey(al => al.AgentId);

            modelBuilder.Entity<AgentListingCase>()
                .HasOne(al => al.ListingCase)
                .WithMany(lc => lc.AgentListingCases)
                .HasForeignKey(al => al.ListingCaseId);
            #endregion
            

            #region Media Asset table
            modelBuilder.Entity<MediaAsset>()
                .HasOne(ma => ma.ListingCase)
                .WithMany(lc => lc.MediaAssets)
                .HasForeignKey(ma => ma.ListingCaseId)
                .OnDelete(DeleteBehavior.NoAction);
            #endregion

            #region Order table
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.ListingCaseId)
                      .IsRequired();
                      
                entity.Property(e => e.AgentId)
                      .IsRequired();
                      
                entity.Property(e => e.Amount)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();
                      
                entity.Property(e => e.Status)
                      .IsRequired();
                      
                entity.HasOne(e => e.ListingCase)
                      .WithMany()
                      .HasForeignKey(e => e.ListingCaseId);
                      
                entity.HasOne(e => e.Agent)
                      .WithMany()
                      .HasForeignKey(e => e.AgentId);
                });
              #endregion


            #region Case Contact table
            modelBuilder.Entity<CaseContact>()
                .HasOne(c => c.ListingCase)
                .WithMany(l => l.CaseContacts)
                .HasForeignKey(c => c.ListingCaseId);
            #endregion

            #region Photography Company Table
            modelBuilder.Entity<PhotographyCompany>()
                .HasOne(a => a.User)
                .WithOne(u => u.PhotographyCompany)
                .HasForeignKey<PhotographyCompany>(a => a.Id)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region Agent Photography Company table
            modelBuilder.Entity<AgentPhotographyCompany>() 
                .HasKey(ag => new
                { //Composite Key
                    ag.AgentId,
                    ag.CompanyId
                });

            modelBuilder.Entity<AgentPhotographyCompany>()
                .HasOne(ap => ap.Agent)
                .WithMany(a => a.AgentPhotographyCompanies)
                .HasForeignKey(ap => ap.AgentId);

            modelBuilder.Entity<AgentPhotographyCompany>()
                .HasOne(ap => ap.PhotographyCompany)
                .WithMany(a => a.AgentPhotographyCompanies)
                .HasForeignKey(ap => ap.CompanyId);
            #endregion

        }
    }
}
