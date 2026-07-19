namespace Server.Domain;

using System.Collections.Generic;

using LanguageExt.Common;

// The Core Domain Base: Implements the abstract parts of LanguageExt.Common.Error natively
public abstract record DomainError(string Message) : Error
{
    public override string Message { get; } = Message;
    public override bool IsExceptional => false;
    public override bool IsExpected => true;

    public override ErrorException ToErrorException() =>
        ErrorException.New(Code, Message);

    public override bool Is<TE>() => false;
}

public record ForbiddenError(string Message) : DomainError(Message)
{
    public override int Code => StatusCodes.Status403Forbidden;
}

// Resource Not Found Variant
public record NotFoundError(string Message) : DomainError(Message)
{
    public override int Code => StatusCodes.Status404NotFound;
}

// Business Constraint / Concurrency / Duplicate Rules Variant
public record ConflictError(string Message) : DomainError(Message)
{
    public override int Code => StatusCodes.Status409Conflict;
}

// Security / Permission Boundary Variant
public record UnauthorizedError(string Message = "Access denied.")
    : DomainError(Message)
{
    public override int Code => StatusCodes.Status401Unauthorized;
}

// Represents a single point of failure
public record ValidationErrorItem(string Property, string Message);

// Input / Complex Rules Validation Variant
public record ValidationError(string Message, IReadOnlyCollection<ValidationErrorItem> Failures)
    : DomainError(Message)
{
    public override int Code => StatusCodes.Status400BadRequest;

    // Single error constructor convenience
    public ValidationError(string propertyName, string errorMessage)
        : this("One or more validation failures occurred.", [new ValidationErrorItem(propertyName, errorMessage)])
    {
    }
}

public static class DomainErrorExtensions
{
    extension(DomainError)
    {
        public static ForbiddenError Forbid(string message) => new(message);
        public static NotFoundError NotFound(string message) => new(message);
        public static ConflictError Conflict(string message) => new(message);
        public static UnauthorizedError Unauthorized(string message = "Access denied.") => new(message);

        public static ValidationError Validation(string propertyName, string message) =>
            new(propertyName, message);

        public static ValidationError Validation(string message, IEnumerable<ValidationErrorItem> failures) =>
            new(message, failures.ToList());
    }

    extension(ValidationError error)
    {
        public ValidationError And(string propertyName, string message) =>
            error with { Failures = error.Failures.Append(new ValidationErrorItem(propertyName, message)).ToList() };
    }
}