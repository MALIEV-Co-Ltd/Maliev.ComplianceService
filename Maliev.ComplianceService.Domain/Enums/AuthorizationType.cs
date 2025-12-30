namespace Maliev.ComplianceService.Domain.Enums;

/// <summary>
/// Defines the types of work authorizations supported by the system.
/// </summary>
public enum AuthorizationType
{
    /// <summary> Citizen of the country. </summary>
    Citizen = 0,
    /// <summary> Permanent resident. </summary>
    PermanentResident = 1,
    /// <summary> Work visa holder. </summary>
    WorkVisa = 2,
    /// <summary> Student visa holder with work authorization. </summary>
    StudentVisa = 3,
    /// <summary> Temporary worker permit. </summary>
    TemporaryWorker = 4,
    /// <summary> Refugee status. </summary>
    Refugee = 5,
    /// <summary> Asylee status. </summary>
    Asylee = 6,
    /// <summary> Other types of work authorization. </summary>
    Other = 7
}
