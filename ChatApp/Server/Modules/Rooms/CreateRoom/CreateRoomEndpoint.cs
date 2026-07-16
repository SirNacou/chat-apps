using System.Security.Claims;

using FastEndpoints;

using Server.Domain;
using Server.Infrastructure.Database;

namespace Server.Modules.Rooms.CreateRoom;

public sealed class CreateRoomEndpoint(ApplicationDbContext dbContext) : Ep.Req<CreateRoomRequest>.NoRes
{
    public override void Configure()
    {
        Post("");
        Group<RoomsGroup>();
    }

    public override async Task HandleAsync(CreateRoomRequest req, CancellationToken ct)
    {
        var room = Room.CreateGroupRoom(req.Name,
            HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(
            new { RoomId = room.Id.ToString() }, ct);
    }
}

public sealed class CreateRoomResponse
{
    public required string RoomId { get; set; }
}