using Maliev.ComplianceService.Domain.Enums;

namespace Maliev.ComplianceService.Application.Commands.RecordWorkAuthorization;

/// <summary>
/// Validator for the RecordWorkAuthorizationCommand.
/// </summary>
public class RecordWorkAuthorizationValidator
{
    /// <summary>
    /// Validates the record work authorization command.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <returns>A validation result containing any errors.</returns>
    public RecordWorkAuthorizationValidationResult Validate(RecordWorkAuthorizationCommand command)
    {
        var request = command.Request;
        var errors = new List<RecordWorkAuthorizationValidationError>();

        // Document number required validation
        if (string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            errors.Add(new RecordWorkAuthorizationValidationError("INVALID_DOCUMENT_NUMBER", "The document number is required."));
        }
        // Document number format validation (FR-024)
        else if (!IsValidDocumentNumber(request.DocumentNumber))
        {
            errors.Add(new RecordWorkAuthorizationValidationError("INVALID_DOCUMENT_NUMBER", "The document number format is invalid."));
        }

        // Date range validation
        if (request.ExpirationDate.HasValue && request.IssueDate > request.ExpirationDate.Value)
        {
            errors.Add(new RecordWorkAuthorizationValidationError("INVALID_DATE_RANGE", "Issue date cannot be after expiration date"));
        }

        return new RecordWorkAuthorizationValidationResult(errors);
    }

    private static bool IsValidDocumentNumber(string? documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber)) return false;

        // Simple validation for alphanumeric document numbers, usually 1-100 characters
        // For specific authorities, we could add more complex rules.
        // E.g., USCIS: 3 letters followed by 10 digits
        return documentNumber.Length >= 1 && documentNumber.Length <= 100;
    }
}

/// <summary>
/// Represents the result of validating a work authorization record command.
/// </summary>
/// <param name="Errors">The list of validation errors.</param>
public record RecordWorkAuthorizationValidationResult(List<RecordWorkAuthorizationValidationError> Errors)
{
    /// <summary>
    /// Gets a value indicating whether the validation passed (no errors).
    /// </summary>
    public bool IsValid => Errors.Count == 0;
}

/// <summary>
/// Represents a single validation error for a work authorization record.
/// </summary>
/// <param name="ErrorCode">The error code identifying the type of validation failure.</param>
/// <param name="Message">A human-readable message describing the validation error.</param>
public record RecordWorkAuthorizationValidationError(string ErrorCode, string Message);