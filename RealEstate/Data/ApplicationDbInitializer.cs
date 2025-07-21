using Microsoft.AspNetCore.Identity;
using RealEstate.Domain;


namespace RealEstate.Data
{
    public static class ApplicationDbInitializer
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

            string[] roleNames = { "Admin", "Agent", "PhotographyCompany" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper()
                    });
                }
            }
        }


    }
}
