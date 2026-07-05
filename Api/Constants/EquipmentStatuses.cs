namespace Uam.AdvancedProgramming.Api.Constants;

public static class EquipmentStatuses
{
    public const string Operational = "Operational";
    public const string UnderRepair = "UnderRepair";

    public static readonly string[] All =
    [
        Operational,
        UnderRepair
    ];
}