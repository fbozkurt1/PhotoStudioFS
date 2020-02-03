using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PhotoStudioFS.Data;
using PhotoStudioFS.Helpers;
using PhotoStudioFS.Helpers.Email;
using PhotoStudioFS.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace PhotoStudioFS
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
            var connection = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<photostudioContext>(
                options =>
                {
                    options.UseMySql(connection);
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                });

            //services.BuildServiceProvider().GetService<photostudioContext>().Database.Migrate();

            services.AddIdentity<User, IdentityRole>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddEntityFrameworkStores<photostudioContext>()
                .AddDefaultTokenProviders();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<AWSModel>(Configuration.GetSection("AWS"));
            services.AddTransient<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                //options.Password.RequireDigit = true;
                //options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                //options.Password.RequireUppercase = true;
                //options.Password.RequiredLength = 6;
                //options.Password.RequiredUniqueChars = 0;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

            });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    // Cookie settings
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.Cookie = new CookieBuilder
                    {
                        Name = "AspNetCoreIdentityCookie", //Oluşturulacak Cookie'yi isimlendiriyoruz.
                        HttpOnly = false, //Kötü niyetli insanların client-side tarafından Cookie'ye erişmesini engelliyoruz.
                        SameSite = SameSiteMode.Lax, //Top level navigasyonlara sebep olmayan requestlere Cookie'nin gönderilmemesini belirtiyoruz.
                        SecurePolicy = CookieSecurePolicy.Always //HTTPS üzerinden erişilebilir yapıyoruz.
                    };
                    options.SlidingExpiration = true;
                });

            //services.ConfigureApplicationCookie(options =>
            //{
            //    // Cookie settings
            //    options.Cookie.HttpOnly = true;
            //    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            //    options.LoginPath = "/Account/Login";
            //    options.LogoutPath = "/Account/Logout";
            //    options.AccessDeniedPath = "/Account/AccessDenied";
            //    options.Cookie = new CookieBuilder
            //    {
            //        Name = "AspNetCoreIdentityCookie", //Oluşturulacak Cookie'yi isimlendiriyoruz.
            //        HttpOnly = false, //Kötü niyetli insanların client-side tarafından Cookie'ye erişmesini engelliyoruz.
            //        SameSite = SameSiteMode.Lax, //Top level navigasyonlara sebep olmayan requestlere Cookie'nin gönderilmemesini belirtiyoruz.
            //        SecurePolicy = CookieSecurePolicy.Always //HTTPS üzerinden erişilebilir yapıyoruz.
            //    };
            //    options.SlidingExpiration = true;
            //});
            //services.BuildServiceProvider().GetService<photostudioContext>().Database.Migrate();
            services.AddAuthorization();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("CoreSwagger", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Swagger on ASP.NET Core",
                    Version = "1.0.0",
                    Description = "Try Swagger on (ASP.NET Core 2.1)",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                    {
                        Name = "Swagger Implementation Fuat Bozkurt",
                        Email = "fuatbozkurt1@gmail.com"
                    },
                    TermsOfService = new System.Uri("http://swagger.io/terms/")
                });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //context.Database.Migrate();
            app.UseSwagger()
            .UseSwaggerUI(c =>
            {
                //TODO: Either use the SwaggerGen generated Swagger contract (generated from C# classes)
                c.SwaggerEndpoint("/swagger/CoreSwagger/swagger.json", "Swagger Test .Net Core");

                //TODO: Or alternatively use the original Swagger contract that's included in the static files
                // c.SwaggerEndpoint("/swagger-original.json", "Swagger Petstore Original");
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                        name: "admin",
                        template: "admin/{controller=Admin}/{action=Index}/{id?}");

                routes.MapRoute(
                        name: "api",
                        template: "{controller=Api}/{action=Index}/{id?}");

            });

            IdentityDataInitializer.SeedDataAsync(serviceProvider).Wait();

        }
    }
}
