using Microsoft.Extensions.Logging;

namespace Identity.Shared.Localization;

public class LocalizationService : LocalizationFileReader, ILocalizationService
{
    public LocalizationService(ILogger<LocalizationFileReader> logger) 
        : base("localizationFile", logger) 
    { 
    }

    public string Get(string key) => GetKeyValue(key, key);
    
    public string Get(string key, string defaultValue) => GetKeyValue(key, defaultValue);
    
    public Dictionary<string, string> GetAll(string key) => GetKeyValue(key);
}
