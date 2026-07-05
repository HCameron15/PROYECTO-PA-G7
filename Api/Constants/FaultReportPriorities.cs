namespace Uam.AdvancedProgramming.Api.Constants;

public static class FaultReportPriorities
{
    public const string Low = "Low";
    public const string Medium = "Medium";
    public const string High = "High";
    public const string Critical = "Critical";

    public static readonly string[] All =
    [
        Low,
        Medium,
        High,
        Critical
    ];
}