namespace TestAutomationPlatform.Services;

public static class CronHelper
{
    public static string ToCronExpression(string interval)
    {
        return interval switch
        {
            "Every5Minutes" => "*/5 * * * *",
            "Every15Minutes" => "*/15 * * * *",
            "Every30Minutes" => "*/30 * * * *",
            "Hourly" => "0 * * * *",
            "Daily" => "0 8 * * *",
            _ => "*/5 * * * *"
        };
    }

    public static string ToDescription(string interval)
    {
        return interval switch
        {
            "Every5Minutes" => "Elke 5 minuten",
            "Every15Minutes" => "Elke 15 minuten",
            "Every30Minutes" => "Elke 30 minuten",
            "Hourly" => "Elk uur",
            "Daily" => "Dagelijks om 08:00",
            _ => "Elke 5 minuten"
        };
    }
}