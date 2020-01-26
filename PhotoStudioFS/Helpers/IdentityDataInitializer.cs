using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PhotoStudioFS.Helpers;
using PhotoStudioFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoStudioFS.Data
{
    public static class IdentityDataInitializer
    {
        private static RoleManager<IdentityRole> RoleManager;
        private static UserManager<User> UserManager;
        private static string[] roleNames = Roles.GetRoles();
        private static IdentityResult roleResult;

        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            UserManager = serviceProvider.GetRequiredService<UserManager<User>>();

            foreach (var roleName in roleNames)
            {
                //creating the roles and seeding them to the database
                var roleExist = await RoleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await RoleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminUser = await UserManager.GetUsersInRoleAsync(Roles.Admin);
            if (!adminUser.Any())
            {
                var user = new User()
                {
                    Email = "fuatbozkurt1@gmail.com",
                    EmailConfirmed = true,
                    FullName = "Fuat Bozkurt",
                    IsEnabled = true,
                    IsFirstLogin = false,
                    PhoneNumber = "05344047939",
                    UserName = "fuatbozkurt1@gmail.com",
                    LockoutEnabled = false
                };
                await UserManager.CreateAsync(user, "Ogvbsiabkckq1");
                await UserManager.AddToRoleAsync(user, Roles.Admin);

            }

        }



    }
}
