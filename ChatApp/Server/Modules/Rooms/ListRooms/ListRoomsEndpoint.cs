using System.Security.Claims;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using Server.Infrastructure.Database;

namespace Server.Modules.Rooms.ListRooms;

public sealed class ListRoomsEndpoint(ApplicationDbContext dbContext) : Ep.NoReq.Res<ListRoomResponse>
{
    public override void Configure()
    {
        Get("");
        Group<RoomsGroup>();
    }

    public override async Task<ListRoomResponse> ExecuteAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            ThrowError("Access denied.", statusCode: StatusCodes.Status401Unauthorized);

        var rooms = await dbContext.RoomMembers
            .Where(rm => rm.UserId == userId)
            .Include(rm => rm.Room)
            .ToListAsync(ct);

        return new ListRoomResponse
        {
            Rooms = rooms.Select(r =>
                    new RoomDto { Id = r.Room.Id.ToString(), Name = r.Room.Name, Type = r.Room.Type.ToString() })
                .ToList()
        };
    }
}

public sealed class ListRoomResponse
{
    public List<RoomDto> Rooms { get; set; } = [];
}

public sealed class RoomDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
}