using Identity.Shared.Identity;
using Identity.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Api.Extensions
{
    public static class SharedDependencyInjection
    {
        public static IServiceCollection AddSharedServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IFileService, FileService>();

            return services;
        }
    }
}
