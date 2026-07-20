namespace Uam.AdvancedProgramming.Api.DTOs.Dashboard;

public record GeneralSummaryDto(
    int TotalReports,
    int PendingCount,
    int InProgressCount,
    int ResolvedCount,
    int ClosedCount,
    int TotalEquipment,
    int EquipmentUnderRepair);
