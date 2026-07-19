using FastEndpoints;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using Server.Common.Extensions;
using Server.Domain;
using Server.Infrastructure.Database;
using Server.Modules.Chat;

namespace Server.Modules.Rooms.SendMessagesOld;

public sealed class SendMessagesEndpointOld(ApplicationDbContext dbContext, IHubContext<ChatHub, IChatClient> hubContext)
    : Ep.Req<SendMessagesRequest>.NoRes
{
    public override void Configure()
    {
        Post("/{roomId}/messagesold");
        Group<RoomsGroup>();
    }

    public override async Task HandleAsync(SendMessagesRequest req, CancellationToken ct)
    {
        var pipeline = User.GetUserId()
            .ToEff(DomainError.Unauthorized())
            .SelectMany(uid => CheckIsMember(uid, req.RoomId, ct), (uid, _) => uid)
            .SelectMany(uid => SaveToDb(req.RoomId, uid, req.Content, ct), (_, message) => message)
            .SelectMany(
                message => Aff(async () => await hubContext.Clients.Group(req.RoomId.ToString())
                    .ReceiveMessage(new MessageResponseDto(message)).ToUnit()),
                (message, _) => message);

        var res = await pipeline.Run();

        await Send.CreatedAtAsync(nameof(SendMessagesEndpointOld), res, cancellation: ct);
    }

    private Aff<Unit> CheckIsMember(string uid, Guid roomId, CancellationToken ct)
    {
        return Aff(async () => await dbContext.RoomMembers.AnyAsync(rm => rm.RoomId == roomId && rm.UserId == uid, ct))
            .Bind(isMember => isMember
                ? unitAff
                : LanguageExt.Aff<Unit>.Fail(
                    DomainError.Forbid("You do not have permission to post to this room.")));
    }

    private Aff<Message> SaveToDb(Guid roomId, string senderId, string content, CancellationToken ct)
    {
        return Aff(async () =>
        {
            var message = Message.Create(roomId, senderId, content);
            dbContext.Messages.Add(message);
            await dbContext.SaveChangesAsync(ct);
            return message;
        });
    }
}

public sealed class SendMessagesRequest
{
    [RouteParam]
    public required Guid RoomId { get; set; }

    public required string Content { get; set; }
}

public sealed class SendMessagesValidator : Validator<SendMessagesRequest>
{
    public SendMessagesValidator()
    {
        RuleFor(x => x.Content).NotEmpty().WithMessage("Content is required.");
    }
}