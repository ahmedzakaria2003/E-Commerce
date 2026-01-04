using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Extensions
{
    public static class AuthenticationExtenstion
    {
        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var key = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "JWT Key is required in configuration");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false; // Set to true in production
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            message = "You are not authorized to access this resource",
                            statusCode = 401
                        });
                        return context.Response.WriteAsync(result);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            message = "You do not have permission to access this resource",
                            statusCode = 403
                        });
                        return context.Response.WriteAsync(result);
                    }
                };
            });

            return services;
        }

        //public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
        //{
        //    services.AddAuthorization(options =>
        //    {
        //        //// Default policy requires authentication
        //        //options.FallbackPolicy = new AuthorizationPolicyBuilder()
        //        //    .RequireAuthenticatedUser()
        //        //    .Build();

        //        options.AddPolicy("AdminOnly", policy =>
        //            policy.RequireRole("Admin"));

        //        options.AddPolicy("TraderOnly", policy =>
        //            policy.RequireRole("Trader"));

        //        options.AddPolicy("AdminOrManager", policy =>
        //            policy.RequireRole("Admin", "Manager"));

        //        options.AddPolicy("CustomerOnly", policy =>
        //            policy.RequireRole("Customer"));

        //        // Claims-based policies
        //        //options.AddPolicy("CanManageProducts", policy =>
        //        //    policy.RequireClaim("permission", "manage_products"));

        //        //options.AddPolicy("CanManageOrders", policy =>
        //        //    policy.RequireClaim("permission", "manage_orders"));

        //        //options.AddPolicy("CanManageUsers", policy =>
        //        //    policy.RequireClaim("permission", "manage_users"));

        //        // Age-based policy example
        //        //options.AddPolicy("MinimumAge18", policy =>
        //        //    policy.RequireAssertion(context =>
        //        //        context.User.HasClaim(c => c.Type == "age" &&
        //        //        int.TryParse(c.Value, out int age) && age >= 18)));
        //    });

        //    return services;
        //}

        //public static IServiceCollection AddCustomAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
        //{
        //    services.AddCustomAuthentication(configuration);
        //    services.AddCustomAuthorization();
        //    return services;
        //}

        public static IApplicationBuilder UseCustomAuthentication(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }
    }
}
