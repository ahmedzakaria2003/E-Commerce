using Identity.Domain.Entities;
using Identity.Infrastrcture.Intersptors;
using Identity.Infrastrcture.Persistence;
using Identity.Shared.Settigns;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Identity.Api.Extensions
{
    public static class WebDependencyInjection
    {
        public static IServiceCollection AddWebServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 1) JWT Settings
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
                ?? throw new Exception("Missing JwtSettings");

            services.AddSingleton(jwtSettings);

            // 2) DbContext with AuditInterceptor
            services.AddDbContext<AppDbContext>((sp, options) =>

            {
                var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                       .AddInterceptors(auditInterceptor);
            });

            // 3) Identity
            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        

            return services;
        }
    }
}

