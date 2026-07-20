using System.Security.Claims;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using Server.Domain;
using Server.Infrastructure.Database;

namespace Server.Modules.Rooms.CreateDirectMessage;

public sealed class CreateDirectMessageEndpoint(ApplicationDbContext dbContext)
    : Ep.Req<CreateDirectMessageRequest>.NoRes
{
    public override void Configure()
    {
        Post("/dm");
        Group<RoomsGroup>();
    }

    public override async Task HandleAsync(CreateDirectMessageRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            ThrowError("User is not authenticated.", statusCode: StatusCodes.Status401Unauthorized);

        var recipientExists = await dbContext.Users.AnyAsync(u => u.Id == req.RecipientId, ct);
        if (!recipientExists)
            ThrowError("The specified recipient does not exist.",
                statusCode: StatusCodes.Status400BadRequest);

        var existingRoomId = await dbContext.Rooms
            .Where(r => r.Type == RoomType.DirectMessage)
            .Where(r => r.Members.Any(m => m.UserId == userId)
                        && r.Members.Any(m => m.UserId == req.RecipientId))
            .Select(r => r.Id.ToString())
            .FirstOrDefaultAsync(ct);

        string roomId;
        if (existingRoomId != null)
        {
            roomId = existingRoomId;
        }
        else
        {
            var newRoom = Room.CreateDirectMessageRoom(userId, req.RecipientId);
            dbContext.Rooms.Add(newRoom);
            await dbContext.SaveChangesAsync(ct);
            roomId = newRoom.Id.ToString();
        }

        await Send.OkAsync(new { RoomId = roomId }, ct);
    }
}