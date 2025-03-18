using System;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(ConvenientCarShare.Areas.Identity.IdentityHostingStartup))]
namespace ConvenientCarShare.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(
                        context.Configuration.GetConnectionString("ApplicationDbContextConnection")));

                /*
                services.AddDefaultIdentity<ApplicationUser>()
                        .AddEntityFrameworkStores<ApplicationDbContext>()
                        ;
                */
                
                services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    //options.SignIn.RequireConfirmedEmail = true;
                })
                        .AddDefaultUI()
                        .AddDefaultTokenProviders()
                        .AddEntityFrameworkStores<ApplicationDbContext>();
                
            });


        }
    }
}