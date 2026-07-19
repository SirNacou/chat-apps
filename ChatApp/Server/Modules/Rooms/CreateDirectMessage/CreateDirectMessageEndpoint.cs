using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using Server.Common.Extensions;
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
        Summary(s => s.ExampleRequest = new CreateDirectMessageRequest());
    }

    public override async Task HandleAsync(CreateDirectMessageRequest req, CancellationToken ct)
    {
        var pipeline =
            from userId in User.GetUserId()
                .ToEff(DomainError.Unauthorized("User is not authenticated."))
            from _ in ValidateRecipientExists(req.RecipientId, ct)
            from roomId in GetOrCreateDirectMessageRoom(userId, req.RecipientId, ct)
            select roomId;

        var result = await pipeline.Run();

        await result.Match(
            async roomId => await Send.OkAsync(new { RoomId = roomId }, ct),
            error => Send.DomainErrorAsync(error, ct));
    }

    private Aff<Unit> ValidateRecipientExists(string recipientId, CancellationToken ct)
    {
        return Aff(async () => await dbContext.Users.AnyAsync(u => u.Id == recipientId, ct))
            .Bind(exists => exists
                ? unitAff
                : FailAff<Unit>(DomainError.Validation(nameof(recipientId),
                    "The specified recipient does not exist.")));
    }

    private Aff<string> GetOrCreateDirectMessageRoom(string currentUserId, string recipientId, CancellationToken ct)
    {
        return Aff(async () =>
        {
            var existingRoomId = await dbContext.Rooms
                .Where(r => r.Type == RoomType.DirectMessage)
                .Where(r => r.Members.Any(m => m.UserId == currentUserId)
                            && r.Members.Any(m => m.UserId == recipientId))
                .Select(r => r.Id.ToString())
                .FirstOrDefaultAsync(ct);

            if (existingRoomId != null)
            {
                return existingRoomId;
            }

            // The Creation: Perform internal domain assembly
            var newRoom = Room.CreateDirectMessageRoom(currentUserId, recipientId);
            dbContext.Rooms.Add(newRoom);
            await dbContext.SaveChangesAsync(ct);

            return newRoom.Id.ToString();
        });
    }
}