using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Security.Claims;

namespace Identity.Shared.Identity
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CurrentUserService> _logger;

        private const string CountryContextKey = "CountryContext";
        private const string CultureContextKey = "CultureContext";

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            ILogger<CurrentUserService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public Guid? GetCurrentUserId()
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;

                if (user?.Identity?.IsAuthenticated != true)
                    return null;

                var idClaim =
                       user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value
                    ?? user.FindFirst("user_id")?.Value;

                return Guid.TryParse(idClaim, out var id) ? id : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user ID");
                return null;
            }
        }

        public string GetCurrentUserName()
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                    return "Unknown";

                return user.FindFirst(ClaimTypes.Name)?.Value
                    ?? user.FindFirst("username")?.Value
                    ?? "Unknown";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting username");
                return "Unknown";
            }
        }

        public string GetCurrentUserRole()
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                    return "Unauthorized";

                return user.FindFirst(ClaimTypes.Role)?.Value
                    ?? user.FindFirst("role")?.Value
                    ?? "Unknown";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role");
                return "Unknown";
            }
        }

        public int? GetCurrentUserCountryId()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.User?.Identity?.IsAuthenticated != true)
                    return null;

                // Cached in HttpContext.Items
                if (httpContext.Items.ContainsKey(CountryContextKey))
                    return httpContext.Items[CountryContextKey] as int?;

                var claim = httpContext.User.FindFirst("country_id")?.Value;

                if (int.TryParse(claim, out int country))
                {
                    httpContext.Items[CountryContextKey] = country;
                    return country;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting country ID");
                return null;
            }
        }

        public string GetCurrentCulture()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                    return "en";

                // Cached
                if (httpContext.Items.ContainsKey(CultureContextKey))
                    return httpContext.Items[CultureContextKey] as string ?? "en";

                string culture = "en";

                // NEW WAY (ASP.NET Core 8/9)
                if (httpContext.Request.Headers.TryGetValue("Accept-Language", out var values))
                {
                    var lang = values.FirstOrDefault()?
                        .Split(',').FirstOrDefault()?.Trim();

                    if (!string.IsNullOrEmpty(lang))
                        culture = lang.StartsWith("ar") ? "ar" : "en";
                }
                else
                {
                    culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ar" ? "ar" : "en";
                }

                httpContext.Items[CultureContextKey] = culture;
                return culture;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining culture");
                return "en";
            }
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
    }
}
