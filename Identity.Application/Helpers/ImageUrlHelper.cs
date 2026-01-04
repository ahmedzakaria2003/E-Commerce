using Microsoft.Extensions.Configuration;

namespace Identity.Application.Helpers
{
    public static class ImageUrlHelper
    {
        private static IConfiguration? _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string GetFullImageUrl<TSource>(TSource source, string propertyName, string folder)
        {
            if (_configuration == null)
                throw new InvalidOperationException("ImageUrlHelper not initialized. Call Initialize in Program.cs.");

            if (source == null) return string.Empty;

            var prop = typeof(TSource).GetProperty(propertyName);
            var value = prop?.GetValue(source)?.ToString();

            if (string.IsNullOrEmpty(value)) return string.Empty;

            var baseUrl = _configuration["ImageBaseUrl"];
            var specficPath = _configuration[$"UploadedFiles:{folder}"];
            return $"{baseUrl}{specficPath}{value}";
        }
    }
}
