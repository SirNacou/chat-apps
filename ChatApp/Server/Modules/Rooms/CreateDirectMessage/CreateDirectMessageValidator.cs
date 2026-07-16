using FastEndpoints;

namespace Server.Modules.Rooms.CreateDirectMessage;

public sealed class CreateDirectMessageValidator : Validator<CreateDirectMessageRequest>
{
    public CreateDirectMessageValidator()
    {
        RuleFor(x => x.RecipientId)
            .NotEmpty()
            .WithMessage("Recipient ID is required.");
    }
}