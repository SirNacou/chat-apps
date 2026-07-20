using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using Server.Infrastructure.Database;
using Server.Services;

namespace Server.Modules.Users.ListUsers;

public sealed class ListUsersEndpoint(ApplicationDbContext dbContext, IPresenceService presenceService)
    : Ep.NoReq.Res<ListUsersResponse>
{
    public override void Configure()
    {
        Get("");
        Group<UsersGroup>();
    }

    public override async Task<ListUsersResponse> ExecuteAsync(CancellationToken ct)
    {
        var users = await dbContext.Users
            .Select(u => new UserResponseDto()
            {
                Id = u.Id, Username = u.UserName ?? string.Empty, LastSeenAt = u.LastSeenAt
            })
            .ToListAsync();

        var onlineUsers = await presenceService.GetOnlineUsersAsync();
        var onlineSet = onlineUsers.ToHashSet();

        foreach (var user in users)
        {
            user.IsOnline = onlineSet.Contains(user.Id);
        }

        return new ListUsersResponse() { Data = users };
    }
}

public sealed class ListUsersResponse
{
    public List<UserResponseDto> Data { get; set; } = [];
}

public record UserResponseDto
{
    public string Id { get; init; } = null!;
    public string Username { get; init; } = null!;
    public DateTimeOffset LastSeenAt { get; init; }
    public bool IsOnline { get; set; }
}