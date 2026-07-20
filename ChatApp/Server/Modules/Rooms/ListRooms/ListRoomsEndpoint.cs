using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using Server.Common.Extensions;
using Server.Domain;
using Server.Infrastructure.Database;

namespace Server.Modules.Rooms.ListRooms;

public sealed class ListRoomsEndpoint(ApplicationDbContext dbContext) : Ep.NoReq.Res<Fin<ListRoomResponse>>
{
    public override void Configure()
    {
        Get("");
        Group<RoomsGroup>();
    }

    public override async Task<Fin<ListRoomResponse>> ExecuteAsync(CancellationToken ct)
    {
        var pipeline = User.GetUserId()
            .ToAff(DomainError.Unauthorized())
            .MapAsync(async uid => await dbContext.Rooms.ToListAsync(ct))
            .Map(rooms => new ListRoomResponse
            {
                Rooms = rooms.Select(r =>
                        new RoomDto { Id = r.Id.ToString(), Name = r.Name, Type = r.Type.ToString() })
                    .ToList()
            });

        return await pipeline.Run();
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