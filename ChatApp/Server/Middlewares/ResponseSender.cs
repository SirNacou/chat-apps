using FastEndpoints;

using FluentValidation.Results;

using LanguageExt.Common;

using Server.Domain;

namespace Server.Middlewares;

public sealed class ResponseSender<T> : IGlobalPostProcessor where T : notnull
{
    public async Task PostProcessAsync(IPostProcessorContext context, CancellationToken ct)
    {
        if (context.HttpContext.ResponseStarted() || context.Response is not Fin<T> fin)
            return;

        await fin.Match(r => context.SendAsync(r, ct),
            e => context.SendDomainErrorAsync(e, ct));
    }
}

public static class PostProcessorContextExtension
{
    extension(IPostProcessorContext ctx)
    {
        public Task SendAsync<T>(T req, CancellationToken ct) where T : notnull
        {
            return ctx.HttpContext.Response.SendAsync(req, cancellation: ct);
        }

        public Task SendDomainErrorAsync(Error error, CancellationToken ct)
        {
            return error switch
            {
                // 1. Explicit Domain Error Matching
                ValidationError ve => ctx.HandleValidation(ve),
                ManyErrors errors => ctx.HandleManyErrors(errors, ct),
                NotFoundError or ConflictError or UnauthorizedError or ForbiddenError =>
                    ctx.HttpContext.Response.SendAsync(new { error.Message }, error.Code, cancellation: ct),

                _ when error.IsExceptional =>
                    ctx.HttpContext.Response.SendAsync(new { Message = "An unexpected server error occurred." },
                        StatusCodes.Status500InternalServerError, cancellation: ct),

                // 3. Built-in LanguageExt Errors with explicitly assigned HTTP Status Codes
                _ when error.Code is >= 400 and < 600 =>
                    ctx.HttpContext.Response.SendAsync(new { error.Message }, error.Code, cancellation: ct),

                // 4. Generic string-only fallbacks (e.g., Error.New("Something failed"))
                _ => ctx.HttpContext.Response.SendAsync(new { error.Message },
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
            return ctx.HttpContext.Response.SendErrorsAsync(failures);
        }

        private Task HandleManyErrors(ManyErrors compoundError, CancellationToken ct)
        {
            // Unpack nested errors into ProblemDetails validations if they are present
            var validationFailures = compoundError.Errors
                .OfType<ValidationError>()
                .SelectMany(ve => ve.Failures)
                .Select(f => new ValidationFailure(f.Property, f.Message))
                .ToList();

            if (validationFailures.Count > 0)
            {
                return ctx.HttpContext.Response.SendErrorsAsync(validationFailures, cancellation: ct);
            }

            // Default compound fallback handling for other generic error collections
            return ctx.HttpContext.Response.SendAsync(
                new { Message = "Multiple operational validation failures occurred." },
                StatusCodes.Status400BadRequest, cancellation: ct);
        }
    }
}