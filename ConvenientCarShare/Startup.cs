using ConvenientCarShare.Services;
using ConvenientCarShare.Services.Email;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ConvenientCarShare
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Bind and validate EmailSender options
            services.AddOptions<EmailSenderOptions>()
                .Bind(Configuration.GetSection("EmailSender"))
                .Validate(options =>
                {
                    return !string.IsNullOrWhiteSpace(options.UserName) &&
                           !string.IsNullOrWhiteSpace(options.Password);
                }, "\n~~Critical Error: EmailSender options UserName and Password must be provided.~~\n" 
                + "You must supply these via environmental variables in your operating system.\n"
                + "Specifically, name them 'EmailSender__UserName' and 'EmailSender__Password'.\n")
                .ValidateOnStart();

            // email sending service
            services.AddTransient<IEmailSender, EmailSender>();

            // email validation (check if env variables are set)
            // Force options validation on startup by adding the hosted service:
            services.AddHostedService<OptionsValidationHostedService>();

            services.AddMvc(); // .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddResponseCompression();
            services.AddResponseCaching();

            services.AddScoped<IBookingsService, BookingsService>();



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {   
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }


            app.UseResponseCompression();
            app.UseResponseCaching();

            app.Use(async (context, next) =>
            {
                // For GetTypedHeaders, add: using Microsoft.AspNetCore.Http;
                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromSeconds(10)
                    };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
                    new string[] { "Accept-Encoding" };

                await next();
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions() {
                OnPrepareResponse = ctx => {

                    ctx.Context.Response.Headers.Append("Cache-Control","public,max-age=600");
                }


            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}"
                );
                endpoints.MapRazorPages();
            });

        }
    }
}
