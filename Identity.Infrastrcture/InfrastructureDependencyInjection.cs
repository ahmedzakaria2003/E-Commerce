using Identity.Domain.Entities;
using Identity.Domain.Interfaces.IRepositories;
using Identity.Infrastrcture.Intersptors;
using Identity.Infrastrcture.Persistence;
using Identity.Infrastrcture.Repositories;
using Identity.Shared.Settigns;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastrcture
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {

            // Register interceptor
            services.AddScoped<AuditInterceptor>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;

        }
    }
}
