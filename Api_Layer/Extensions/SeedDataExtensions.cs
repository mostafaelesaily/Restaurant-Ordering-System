using Data_Access_Layer.Seed;
using Microsoft.AspNetCore.Identity;

namespace Api_Layer.Extensions
{
    public static class SeedDataExtensions
    {
        public static async Task SeedDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var roleManager =
                scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await RoleSeeder.SeedRoleAsync(roleManager);
        }
    }
}
