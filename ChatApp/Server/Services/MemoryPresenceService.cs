namespace Server.Services;

public sealed class MemoryPresenceService : IPresenceService
{
    private readonly Dictionary<string, System.Collections.Generic.HashSet<string>> _userConnections = new();

    public Task<bool> UserConnectedAsync(string userId, string connectionId)
    {
        bool becameOnline = false;
        lock (_userConnections)
        {
            if (!_userConnections.TryGetValue(userId, out var connections))
            {
                connections = [];
                _userConnections[userId] = connections;
                becameOnline = true;
            }

            connections.Add(connectionId);
        }

        return Task.FromResult(becameOnline);
    }

    public Task<bool> UserDisconnectedAsync(string userId, string connectionId)
    {
        bool wentOffline = false;

        lock (_userConnections)
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);

                // If this was the user's last active tab/connection, this is a 1 -> 0 transition
                if (connections.Count == 0)
                {
                    _userConnections.Remove(userId);
                    wentOffline = true; // User just transitioned to OFFLINE ⚪
                }
            }
        }

        return Task.FromResult(wentOffline);
    }

    public Task<string[]> GetOnlineUsersAsync()
    {
        lock (_userConnections)
        {
            // Return a snapshot copy of all active online user IDs
            return Task.FromResult(_userConnections.Keys.ToArray());
        }
    }

    public Task<bool> IsUserOnlineAsync(string userId)
    {
        lock (_userConnections)
        {
            return Task.FromResult(_userConnections.ContainsKey(userId));
        }
    }
}