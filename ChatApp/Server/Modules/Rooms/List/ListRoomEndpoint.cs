using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using Server.Infrastructure.Database;

namespace Server.Modules.Rooms.List;

public sealed class ListRoomEndpoint(ApplicationDbContext dbContext) : Ep.NoReq.Res<ListRoomResponse>
{
    public override void Configure()
    {
        Get("");
        Group<RoomsGroup>();
    }

    public override async Task<ListRoomResponse> ExecuteAsync(CancellationToken ct)
    {
        var rooms = await dbContext.Rooms.ToListAsync(ct);

        return new ListRoomResponse
        {
            Rooms = rooms.Select(r =>
                    new RoomDto { Id = r.Id.ToString(), Name = r.Name, Type = r.Type.ToString() })
                .ToList()
        };
    }
}

public sealed class ListRoomResponse
{
    public required List<RoomDto> Rooms { get; set; }
}

public sealed class RoomDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
}