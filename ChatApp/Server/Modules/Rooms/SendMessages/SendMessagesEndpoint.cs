using FastEndpoints;

namespace Server.Modules.Rooms.SendMessages;

public class SendMessagesEndpoint
    : Ep.Req<SendMessagesRequest>.NoRes
{
    public override void Configure()
    {
        Post("/{roomId}/messages");
        Group<RoomsGroup>();
    }

    public override async Task HandleAsync(SendMessagesRequest req, CancellationToken ct)
    {
        await Send.OkAsync(null, ct);
    }

}

public class SendMessagesRequest
{
    public string Content { get; set; } = null!;
}

public sealed class SendMessagesValidator : Validator<SendMessagesRequest>
{
    public SendMessagesValidator()
    {
        RuleFor(x => x.Content).NotEmpty().WithMessage("Content is required.");
    }
}