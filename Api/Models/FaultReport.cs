using Uam.AdvancedProgramming.Api.Constants;

namespace Uam.AdvancedProgramming.Api.Models;

public class FaultReport
{
    public int Id { get; set; }

    public int EquipmentId { get; set; }

    public int ReportedByUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string Status { get; set; } = FaultReportStatuses.Pending;

    public DateTime ReportedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public Equipment Equipment { get; set; } = null!;

    public User ReportedByUser { get; set; } = null!;
}