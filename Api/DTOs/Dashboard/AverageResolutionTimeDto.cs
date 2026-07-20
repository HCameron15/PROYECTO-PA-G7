namespace Uam.AdvancedProgramming.Api.DTOs.Dashboard;

public record AverageResolutionTimeDto(
    double AverageHours,
    double FastestResolutionHours,
    double SlowestResolutionHours);
