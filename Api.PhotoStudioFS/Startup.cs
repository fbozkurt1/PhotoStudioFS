using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PhotoStudioFS.Data;
using PhotoStudioFS.Helpers.Email;
using PhotoStudioFS.Models;

namespace Api.PhotoStudioFS
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

            services.AddIdentity<User, IdentityRole>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddEntityFrameworkStores<photostudioContext>()
                .AddDefaultTokenProviders();
            services.AddTransient<IRazorViewToStringRenderer, RazorViewToStringRenderer>();

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(cfg =>
               {
                   cfg.RequireHttpsMetadata = false;
                   cfg.SaveToken = true;
                   cfg.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidIssuer = Configuration["JwtIssuer"],
                       ValidAudience = Configuration["JwtIssuer"],
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtKey"])),
                       ClockSkew = TimeSpan.Zero // remove delay of token when expire
                   };
               });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("CoreSwagger", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "API documentation for sinemkobaner.com",
                    Version = "1.0.0",
                    Description = "Try Swagger on (ASP.NET Core 2.2)",
                    Contact = new OpenApiContact()
                    {
                        Name = "Swagger Implementation By Fuat Bozkurt",
                        Email = "fuatbozkurt1@gmail.com"
                    },
                    TermsOfService = new System.Uri("http://swagger.io/terms/")
                });
                c.AddSecurityDefinition("Bearer",
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
                    {
                        In = ParameterLocation.Header,
                        Description = "Please enter into field the word 'Bearer' following by space and JWT",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey
                    });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
                {
                      new OpenApiSecurityScheme
                      {
                        Reference = new OpenApiReference()
                          {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                          },
                          Scheme = "oauth2",
                          Name = "Bearer",
                          In = ParameterLocation.Header,

                        },
                        new List<string>()
                        }

                    });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSwagger()
           .UseSwaggerUI(c =>
           {
               //TODO: Either use the SwaggerGen generated Swagger contract (generated from C# classes)
               c.SwaggerEndpoint("/swagger/CoreSwagger/swagger.json", "Swagger Test .Net Core");

               //TODO: Or alternatively use the original Swagger contract that's included in the static files
               // c.SwaggerEndpoint("/swagger-original.json", "Swagger Petstore Original");
           });


            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                        name: "default",
                        template: "{controller=Api}/{action=Index}/{id?}");

            });
        }
    }
}
