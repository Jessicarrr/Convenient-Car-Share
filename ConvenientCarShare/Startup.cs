
using ConvenientCarShare.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddTransient<IEmailSender, EmailSender>(i =>
                            new EmailSender(
                                Configuration["EmailSender:Host"],
                                Configuration.GetValue<int>("EmailSender:Port"),
                                Configuration.GetValue<bool>("EmailSender:EnableSSL"),
                                Configuration["EmailSender:UserName"],
                                Configuration["EmailSender:Password"]
                            )
                        );
            


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddResponseCompression();
            services.AddResponseCaching();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Customer}/{action=Index}/{id?}");
            });
        }
    }
}
