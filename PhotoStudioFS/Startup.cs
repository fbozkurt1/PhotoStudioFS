using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
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
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PhotoStudioFS.Data;
using PhotoStudioFS.Helpers;
using PhotoStudioFS.Helpers.Email;
using PhotoStudioFS.Models;

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
            services.AddResponseCaching(options =>
            {
                options.MaximumBodySize = 1024;
                options.UseCaseSensitivePaths = true;
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.MimeTypes = new[] { "text/plain",
                                            "text/css",
                                            "application/javascript",
                                            "text/html",
                                            "application/xml",
                                            "text/xml",
                                            "application/json",
                                            "text/json",
                                            "image/*"};
                options.Providers.Add<GzipCompressionProvider>();
            });

            var connection = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<photostudioContext>(options =>
                {
                    options.UseMySql(connection);
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                });

            services.AddIdentity<User, IdentityRole>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddEntityFrameworkStores<photostudioContext>()
                .AddDefaultTokenProviders();

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromDays(7);
            });

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

            services.AddAuthorization();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            app.UseResponseCompression();

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

            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    const int durationInSeconds = 60 * 60 * 24 * 365;
                    ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] = "public,max-age=" + durationInSeconds;
                    ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] = new string[] { "Accept-Encoding" };
                    ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Expires] = DateTime.UtcNow.AddYears(1).ToString("R");
                }
            });
            app.UseResponseCaching();

            //app.Use(async (context, next) =>
            //{
            //    context.Response.GetTypedHeaders().CacheControl =
            //        new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
            //        {
            //            Public = true,
            //            MaxAge = TimeSpan.FromSeconds(10),
            //        };
            //    context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
            //        new string[] { "Accept-Encoding" };

            //    await next();
            //});

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

            });

            //IdentityDataInitializer.SeedDataAsync(serviceProvider).Wait();

        }
    }
}
