using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Identity.Shared.Localization;

public class LocalizationFileReader
{
    private readonly ILogger<LocalizationFileReader> _logger;

    #region Prop
    private List<LocalizationFileDataDto> LocalizationDataList { get; set; } = [];
    #endregion

    #region Ctor
    public LocalizationFileReader(string fileName, ILogger<LocalizationFileReader> logger)
    {
        _logger = logger;
        LoadData(fileName);
    }
    #endregion

    #region Load Data
    private void LoadData(string fileName)
    {
        try
        {
            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyLocation!, "Localization", $"{fileName}.json");

            // If not found, try project root structure
            if (!File.Exists(filePath))
            {
                var currentDir = new DirectoryInfo(assemblyLocation!);
                while (currentDir != null && !currentDir.GetDirectories("Identity.Shared").Any())
                {
                    currentDir = currentDir.Parent;
                }

                if (currentDir != null)
                {
                    filePath = Path.Combine(
                        currentDir.FullName,
                        "Identity.Shared",
                        "Localization",
                        $"{fileName}.json"
                    );
                }
            }

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Localization file not found at path: {FilePath}. Using empty localization.", filePath);
                LocalizationDataList = [];
                return;
            }

            var json = File.ReadAllText(filePath);
            LocalizationDataList = JsonSerializer.Deserialize<List<LocalizationFileDataDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? [];

            _logger.LogInformation("Localization file loaded successfully from {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while loading localization file {FileName}. Using empty localization.", fileName);
            LocalizationDataList = [];
        }
    }
    #endregion

    #region Get Data
    protected Dictionary<string, string> GetKeyValue(string key)
    {
        return LocalizationDataList.FirstOrDefault(k => k.Key == key)?.LocalizedValue
               ?? new Dictionary<string, string>();
    }

    protected string GetKeyValue(string key, string altValue)
    {
        string value = altValue;

        var localizedData = LocalizationDataList
            .FirstOrDefault(k => k.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
            ?.LocalizedValue;

        if (localizedData != null && localizedData.TryGetValue(CultureInfo.CurrentCulture.Name, out var localizedValue))
        {
            value = localizedValue;
        }

        return value;
    }
    #endregion
}
