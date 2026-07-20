using System.Security.Claims;

using FastEndpoints;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using Server.Domain;
using Server.Infrastructure.Database;
using Server.Modules.Chat;

namespace Server.Modules.Rooms.SendMessages;

public sealed class SendMessagesEndpoint(ApplicationDbContext dbContext, IHubContext<ChatHub, IChatClient> hubContext)
    : Ep.Req<SendMessagesRequest>.NoRes
{
    public override void Configure()
    {
        Post("/{RoomId}/messages");
        Group<RoomsGroup>();
    }

    public override async Task HandleAsync(SendMessagesRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            ThrowError("Access denied.", statusCode: StatusCodes.Status401Unauthorized);

        var isMember = await dbContext.RoomMembers
            .AnyAsync(rm => rm.RoomId == req.RoomId && rm.UserId == userId, ct);

        if (!isMember)
            ThrowError("You do not have permission to post to this room.",
                statusCode: StatusCodes.Status403Forbidden);

        var message = Message.Create(req.RoomId, userId, req.Content);
        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync(ct);

        await hubContext.Clients.Group(req.RoomId.ToString())
            .ReceiveMessage(new MessageResponseDto(message));

        await Send.CreatedAtAsync(nameof(SendMessagesEndpoint), message, cancellation: ct);
    }
}

public sealed class SendMessagesRequest
{
    [RouteParam]
    public Guid RoomId { get; set; }

    public string Content { get; set; } = string.Empty;
}