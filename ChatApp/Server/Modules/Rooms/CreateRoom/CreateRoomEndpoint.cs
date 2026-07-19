using System.Diagnostics;

using FastEndpoints;

using Server.Common.Extensions;
using Server.Domain;
using Server.Infrastructure.Database;

namespace Server.Modules.Rooms.CreateRoom;

public sealed class CreateRoomEndpoint(ApplicationDbContext dbContext)
    : Ep.Req<CreateRoomRequest>.Res<Fin<CreateRoomResponse>>
{
    public override void Configure()
    {
        Post("");
        Group<RoomsGroup>();
    }

    public override async Task<Fin<CreateRoomResponse>> ExecuteAsync(CreateRoomRequest req, CancellationToken ct)
    {
        var pipeline =
            from uid in User.GetUserId().ToAff(DomainError.Unauthorized())
            let room = Room.CreateGroupRoom(req.Name, uid)
            from savedRoom in SaveRoomToDb(room, ct)
            let res = new CreateRoomResponse() { RoomId = savedRoom.Id.ToString() }
            select res;

        return await pipeline.Run();
    }

    private Aff<Room> SaveRoomToDb(Room room, CancellationToken ct) =>
        Aff(async () =>
        {
            dbContext.Rooms.Add(room);
            await dbContext.SaveChangesAsync(ct);
            return room;
        });
}

public sealed class CreateRoomRequest
{
    public required string Name { get; set; }
}

public sealed class CreateRoomResponse
{
    public required string RoomId { get; set; }
}