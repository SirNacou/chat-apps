using FastEndpoints;

using FluentValidation.Results;

using LanguageExt.Common;

using Server.Domain;

namespace Server.Common.Extensions;

public static class SendExtensions
{
    extension(IResponseSender sender)
    {
        public Task DomainErrorAsync(Error error, CancellationToken ct)
        {
            return error switch
            {
                // 1. Explicit Domain Error Matching
                ValidationError ve => sender.HandleValidation(ve),
                NotFoundError or ConflictError or UnauthorizedError =>
                    sender.HttpContext.Response.SendAsync(new { error.Message }, error.Code, cancellation: ct),

                _ when error.IsExceptional =>
                    sender.HttpContext.Response.SendAsync(new { Message = "An unexpected server error occurred." },
                        StatusCodes.Status500InternalServerError, cancellation: ct),

                // 3. Built-in LanguageExt Errors with explicitly assigned HTTP Status Codes
                _ when error.Code is >= 400 and < 600 =>
                    sender.HttpContext.Response.SendAsync(new { error.Message }, error.Code, cancellation: ct),

                // 4. Generic string-only fallbacks (e.g., Error.New("Something failed"))
                _ => sender.HttpContext.Response.SendAsync(new { error.Message },
                    StatusCodes.Status500InternalServerError, cancellation: ct)
            };
        }

        private Task HandleValidation(ValidationError error)
        {
            // Map your Domain Failures to FluentValidation failures
            var failures = error.Failures
                .Select(f => new ValidationFailure(f.Property, f.Message))
                .ToList();

            // SendErrorsAsync automatically produces a 400 Bad Request with ProblemDetails
            return sender.HttpContext.Response.SendErrorsAsync(failures);
        }
    }
}