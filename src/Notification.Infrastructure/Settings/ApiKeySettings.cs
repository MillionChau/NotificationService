namespace Notification.Infrastructure.Settings;

public class ApiKeySettings
{
    public const string SectionName = "ApiKey";
    public const string HeaderName = "X-Api-Key";

    public string Key { get; set; } = string.Empty;
}
