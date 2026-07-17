using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using Server.Infrastructure.Database;

namespace Server.Modules.Chat;

[Authorize]
public sealed class ChatHub(ApplicationDbContext dbContext) : Hub
{
    public async Task JoinRoom(string roomId)
    {
        if (!Guid.TryParse(roomId, out var parsedRoomId))
        {
            throw new HubException("Invalid room ID format.");
        }

        var currentUserId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(currentUserId))
        {
            throw new HubException("User is not authenticated.");
        }

        var isMember =
            await dbContext.RoomMembers.AnyAsync(rm => rm.UserId == currentUserId && rm.RoomId == parsedRoomId);
        if (!isMember)
        {
            throw new HubException("User is not a member of the room.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
    }
}