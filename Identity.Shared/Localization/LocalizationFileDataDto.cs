namespace Identity.Shared.Localization;

public class LocalizationFileDataDto
{
    public string Key { get; set; } = string.Empty;
    public Dictionary<string, string> LocalizedValue { get; set; } = new();
}
