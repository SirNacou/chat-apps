using Microsoft.AspNetCore.Identity;

namespace Server.Domain;

public class ChatUser : IdentityUser
{
    public DateTimeOffset LastSeenAt { get; set; } = DateTimeOffset.UtcNow;
}