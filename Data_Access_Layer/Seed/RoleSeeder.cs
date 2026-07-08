using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data_Access_Layer.Seed
{
    public class RoleSeeder
    {
      public async Task SeedRoleAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = {"Admin", "Manager", "Cheif","Delivery" };
            
            foreach(var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
