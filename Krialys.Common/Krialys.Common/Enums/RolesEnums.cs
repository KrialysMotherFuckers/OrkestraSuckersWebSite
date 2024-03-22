namespace Krialys.Common.Enums;

/// <summary>
/// Roles (both enum and string array)
/// </summary>
public static class RolesEnums
{
    /// <summary>
    /// Roles enum as string array
    /// </summary>
    public static string[] RolesList = Enum.GetNames(typeof(RolesValues));

    /// <summary>
    /// Roles enum
    /// </summary>
    [Flags]
    public enum RolesValues : byte
    {
        None = 0b_0000_0000,         // 0 no access at all
        Viewer = 0b_0000_0001,       // 1
        Creator = 0b_0000_0010,      // 2
        DataDriven = 0b_0000_0100,   // 4
        AdminExploit = 0b_0000_1000, // 8
        AdminRefs = 0b_0001_0000,    // 16
        SuperAdmin = 0b_0010_0000,   // 32
    }

    /// <summary>
    /// Generic roles enum, sum is 255 (1 + 2 + 4 + 8 + 16 + 32 + 64 + 128) which is MaxValue for a byte
    /// </summary>
    [Flags]
    public enum GenericRolesValues : byte
    {
        R0 = 0b_0000_0000, // 0 no access at all
        R1 = 0b_0000_0001, // 1 READ
        R2 = 0b_0000_0010, // 2 WRITE
        R3 = 0b_0000_0100, // 4 CREATE
        R4 = 0b_0000_1000, // 8 UPDATE
        R5 = 0b_0001_0000, // 16 DELETE
        R6 = 0b_0010_0000, // 32 DATADRIVEN
        R7 = 0b_0100_0000, // 64 ADMIN
    }
}