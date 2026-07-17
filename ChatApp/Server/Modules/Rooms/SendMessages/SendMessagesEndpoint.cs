using System.Security.Claims;

using FastEndpoints;

namespace Server.Modules.Rooms.SendMessages;

public sealed class SendMessagesEndpoint : Ep.Req<SendMessagesRequest>.NoRes
{
    public override void Configure()
    {
        Post("/{roomId}/messages");
        Group<RoomsGroup>();
    }

    public override Task HandleAsync(SendMessagesRequest req, CancellationToken ct)
    {
        User.FindFirst(ClaimTypes.NameIdentifier);

        return base.HandleAsync(req, ct);
    }
}

public sealed class SendMessagesRequest
{
    [RouteParam]
    public required string RoomId { get; set; }

    public required string Content { get; set; }
}

public sealed class SendMessagesValidator : Validator<SendMessagesRequest>
{
    public SendMessagesValidator()
    {
        RuleFor(x => x.RoomId).NotEmpty().WithMessage("RoomId is required.");
        RuleFor(x => x.Content).NotEmpty().WithMessage("Content is required.");
    }
}