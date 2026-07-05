namespace Uam.AdvancedProgramming.Api.Constants;

public static class FaultReportStatuses
{
    public const string Pending = "Pending";
    public const string InProgress = "InProgress";
    public const string Resolved = "Resolved";
    public const string Closed = "Closed";

    public static readonly string[] All =
    [
        Pending,
        InProgress,
        Resolved,
        Closed
    ];
}