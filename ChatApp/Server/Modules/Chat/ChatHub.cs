using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using Server.Domain;
using Server.Infrastructure.Database;
using Server.Services;

namespace Server.Modules.Chat;

[Authorize]
public sealed class ChatHub(IPresenceService presenceService, ApplicationDbContext dbContext) : Hub<IChatClient>
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            // Dictionary checks if 0 -> 1 connections
            var becameOnline = await presenceService.UserConnectedAsync(userId, Context.ConnectionId);
            if (becameOnline)
            {
                await Clients.Others.UserConnected(userId);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            // Dictionary checks if 1 -> 0 connections
            var wentOffline = await presenceService.UserDisconnectedAsync(userId, Context.ConnectionId);
            if (wentOffline)
            {
                var timestamp = DateTimeOffset.UtcNow;

                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    user.LastSeenAt = timestamp;
                    await dbContext.SaveChangesAsync();
                }

                await Clients.Others.UserDisconnected(userId, timestamp);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

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

    public async Task SendMessageToRoom(MessageResponseDto message)
    {
        await Clients.OthersInGroup(message.RoomId.ToString()).ReceiveMessage(message);
    }
}

public interface IChatClient
{
    Task ReceiveMessage(MessageResponseDto message);
    Task UserConnected(string userId);
    Task UserDisconnected(string userId, DateTimeOffset timestamp);
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