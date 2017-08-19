using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleCMSForCore2.Helper;
using SimpleCMSForCore2.Models;
using System;
using Microsoft.AspNetCore.Http;
using NLog.Extensions.Logging;
using NLog.Web;
using SimpleCMSForCore2.Migrations;
using SimpleCMSForCore2.Models.Setting;

namespace SimpleCMSForCore2
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Upload>(Configuration.GetSection("Upload"));

            //call this in case you need aspnet-user-authtype/aspnet-user-identity
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")).UseLoggerFactory(ApplicationLogging.LoggerFactory)
                );


            services.AddIdentity<ApplicationUser, ApplicationRole>(config=>
                {
                    config.User.RequireUniqueEmail = false;
                    config.Password.RequireDigit = Configuration.GetValue<bool>("Password:RequireDigit");
                    config.Password.RequiredLength = Configuration.GetValue<int>("Password:RequiredLength");
                    config.Password.RequireNonAlphanumeric = Configuration.GetValue<bool>("Password:RequireNonAlphanumeric");
                    config.Password.RequireLowercase = Configuration.GetValue<bool>("Password:RequireLowercase");
                    config.Password.RequireUppercase = Configuration.GetValue<bool>("Password:RequireUppercase");
                    config.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(Configuration.GetValue<int>("Lockout:DefaultLockoutTimeSpan"));
                    config.Lockout.MaxFailedAccessAttempts = Configuration.GetValue<int>("Lockout:MaxFailedAccessAttempts");
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
               
            services.AddMvc();

            services.AddSession();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            env.ConfigureNLog("nlog.config");

            loggerFactory.AddNLog();

            app.AddNLogWeb();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("#page505");
            }

            DbInitializer.Initialize(app.ApplicationServices);
            //call ConfigureLogger in a centralized place in the code
            ApplicationLogging.ConfigureLogger(loggerFactory);
            //set it as the primary LoggerFactory to use everywhere
            ApplicationLogging.LoggerFactory = loggerFactory;

            app.UseStaticFiles();
            app.UseSession();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
