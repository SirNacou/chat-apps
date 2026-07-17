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
        var creatorId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(creatorId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var recipientExists = await dbContext.Users.AnyAsync(u => u.Id == req.RecipientId, ct);
        if (!recipientExists)
        {
            ThrowError("The specified recipient does not exist.", "RecipientNotFound",
                statusCode: StatusCodes.Status400BadRequest);
            return;
        }

        var room = Room.CreateDirectMessageRoom(creatorId, req.RecipientId);
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(new { RoomId = room.Id.ToString() }, ct);
    }
}