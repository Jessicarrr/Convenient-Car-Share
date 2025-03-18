using System;
using System.Linq;
using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Controllers;
using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ConvenientCarShare.Data
{
    public static class SeedAdminData
    {
        public static async Task InitializeAsync(
            IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();

            var store = new RoleStore<IdentityRole>(context);
            await EnsureRolesAsync(store, context);

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = services
                .GetRequiredService<UserManager<ApplicationUser>>();
            await EnsureTestAdminAsync(userManager, userStore, context);

        }
        private static async Task EnsureRolesAsync(
            RoleStore<IdentityRole> store, ApplicationDbContext context)
        {
            if (context.Roles.Where(f => f.Name == Constants.AdministratorRole)
                .Count() <= 0)
            {
                var role = new IdentityRole(Constants.AdministratorRole);
                role.NormalizedName = Constants.AdministratorRole.ToUpper().Normalize();

                await store.CreateAsync(role);
            }

            if (context.Roles.Where(f => f.Name == Constants.CustomerRole)
                .Count() <= 0)
            {
                var role = new IdentityRole(Constants.CustomerRole);
                role.NormalizedName = Constants.CustomerRole.ToUpper().Normalize();

                await store.CreateAsync(role);
            }

            context.SaveChanges(); // save to db
        }
        private static async Task EnsureTestAdminAsync(UserManager<ApplicationUser> userManager,
            UserStore<ApplicationUser> store, ApplicationDbContext context)
        {
            ApplicationUser testAdmin = null;

            try
            {
                testAdmin = await userManager.Users
                   .Where(x => x.UserName == "admin@todo.local")
                   .SingleOrDefaultAsync();

                if (testAdmin != null) return;

                testAdmin = new ApplicationUser
                {
                    UserName = "admin@todo.local",
                    Email = "admin@todo.local",
                };

                try
                {
                    Task<IdentityResult> taskResult = userManager.CreateAsync(
                        testAdmin, "NotSecure123!!");
                    taskResult.Wait();
                    var result = taskResult.Result;

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(
                            testAdmin, Constants.AdministratorRole);
                        context.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    // this exception is called when there is a db error.
                    // can be caused by adding a new attribute to the user
                    // without editing the above new ApplicationUser
                    string msg = e.Message;
                }
            }
            catch(Exception e2)
            {
                // can be caused by .SingleOrDefaultAsync returning more than one value.
                string msg2 = e2.Message;
            }
            

            
        }
    }
}
