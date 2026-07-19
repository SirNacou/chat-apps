using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using Server.Domain;
using Server.Infrastructure.Database;

namespace Server.Modules.Chat;

[Authorize]
public sealed class ChatHub : Hub<IChatClient>
{
    public async Task JoinRoom(string roomId, [FromServices] ApplicationDbContext dbContext)
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

    public async Task SendMessageToRoom(MessageResponseDto message)
    {
        await Clients.OthersInGroup(message.RoomId.ToString()).ReceiveMessage(message);
    }
}

public interface IChatClient
{
    Task ReceiveMessage(MessageResponseDto message);
}

public sealed record MessageResponseDto(
    Guid Id,
    Guid RoomId,
    string SenderId,
    string Content,
    DateTimeOffset CreatedAt)
{
    public MessageResponseDto(Message message) : this(
        message.Id,
        message.RoomId,
        message.SenderId,
        message.Content,
        message.CreatedAt)
    {
    }
};