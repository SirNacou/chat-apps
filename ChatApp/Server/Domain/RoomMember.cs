using Microsoft.AspNetCore.Identity;

namespace Server.Domain;

public sealed class RoomMember
{
    public string UserId { get; init; } = null!;

    public Guid RoomId { get; init; }

    public Room Room { get; init; } = null!;
}