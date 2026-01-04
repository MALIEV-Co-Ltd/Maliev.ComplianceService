namespace Maliev.ComplianceService.Application.Commands.ResolveAlert;

/// <summary>
/// Validator for the ResolveAlertCommand.
/// </summary>
public class ResolveAlertValidator
{
    /// <summary>
    /// Validates the resolve alert command.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <returns>True if valid, otherwise false.</returns>
    public bool Validate(ResolveAlertCommand command)
    {
        return !string.IsNullOrWhiteSpace(command.Request.ResolutionNotes) && command.Request.ResolutionNotes.Length >= 10;
    }
}
