namespace VaporNotes.Api.Support;
public static class IConfigurationExtensions
{
    public static string GetRequiredSettingValue(this IConfiguration source, string settingName)
    {
        var value = source[settingName];
        if (value == null)
            throw new Exception($"Missing required setting {settingName}");
        return value;
    }
}