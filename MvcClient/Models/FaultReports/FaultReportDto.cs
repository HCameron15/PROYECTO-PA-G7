namespace Uam.AdvancedProgramming.MvcClient.Models.FaultReports;

public class FaultReportDto
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public int LaboratoryId { get; set; }
    public string LaboratoryName { get; set; } = string.Empty;
    public int ReportedByUserId { get; set; }
    public string ReportedByName { get; set; } = string.Empty;
    public string ReportedByEmail { get; set; } = string.Empty;
    public int? AssignedTechnicianId { get; set; }
    public string AssignedTechnicianName { get; set; } = string.Empty;
    public string AssignedTechnicianEmail { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ReportedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}