namespace Identity.Shared.Localization;

public interface ILocalizationService
{
    string Get(string key);
    string Get(string key, string defaultValue);
    Dictionary<string, string> GetAll(string key);
}
