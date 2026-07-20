namespace Server.Services;

public interface IPresenceService
{
    Task<bool> UserConnectedAsync(string userId, string connectionId);
    Task<bool> UserDisconnectedAsync(string userId, string connectionId);
    Task<string[]> GetOnlineUsersAsync();
    Task<bool> IsUserOnlineAsync(string userId);
}