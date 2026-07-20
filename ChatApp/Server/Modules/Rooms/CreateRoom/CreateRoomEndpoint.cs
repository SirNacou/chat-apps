using System.Security.Claims;

using FastEndpoints;

using Server.Domain;
using Server.Infrastructure.Database;

namespace Server.Modules.Rooms.CreateRoom;

public sealed class CreateRoomEndpoint(ApplicationDbContext dbContext)
    : Ep.Req<CreateRoomRequest>.Res<CreateRoomResponse>
{
    public override void Configure()
    {
        Post("");
        Group<RoomsGroup>();
    }

    public override async Task<CreateRoomResponse> ExecuteAsync(CreateRoomRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            ThrowError("Access denied.", statusCode: StatusCodes.Status401Unauthorized);

        var room = Room.CreateGroupRoom(req.Name, userId);
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync(ct);

        return new CreateRoomResponse { RoomId = room.Id.ToString() };
    }
}

public sealed class CreateRoomRequest
{
    public required string Name { get; set; }
}

public sealed class CreateRoomResponse
{
    public required string RoomId { get; set; }
}