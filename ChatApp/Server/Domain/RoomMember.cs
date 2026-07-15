using Microsoft.AspNetCore.Identity;

namespace Server.Domain;

public class RoomMember
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public Guid RoomId { get; set; }
}
