using System.ComponentModel;
using System.Globalization;
using System.Text;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using Server.Infrastructure.Database;

namespace Server.Modules.Rooms.GetMessages;

public sealed class GetMessagesEndpoint(ApplicationDbContext dbContext)
    : Ep.Req<GetMessagesRequest>.Res<GetMessagesResponse>
{
    public override void Configure()
    {
        Get("/{roomId}/messages");
        Group<RoomsGroup>();
    }

    public override async Task<GetMessagesResponse> ExecuteAsync(GetMessagesRequest req, CancellationToken ct)
    {
        var roomId = Route<Guid>("roomId");
        ThrowIfAnyErrors();

        var cursor = req.Cursor;
        var limit = req.Limit;

        if (limit <= 0) limit = 50;
        if (limit > 100) limit = 100;

        var query = dbContext.Messages.Where(r => r.RoomId == roomId);

        if (!string.IsNullOrEmpty(cursor))
        {
            var (cursorCreatedAt, cursorId) = ParseCursor(cursor);
            query = query.Where(m =>
                m.CreatedAt < cursorCreatedAt || (m.CreatedAt == cursorCreatedAt && m.Id < cursorId));
        }

        var messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .Take(limit + 1)
            .ToListAsync(ct);

        var hasNextPage = messages.Count > limit;
        string? nextCursor = null;

        if (hasNextPage)
        {
            messages.RemoveAt(messages.Count - 1);

            var lastMessage = messages[^1];

            nextCursor = CreateCursor(lastMessage.CreatedAt, lastMessage.Id);
        }

        messages.Reverse();

        return new GetMessagesResponse
        {
            NextCursor = nextCursor,
            HasNextPage = hasNextPage,
            Messages = messages.Select(m =>
                    new GetMessagesResponse.MessageDto
                    {
                        Id = m.Id.ToString(),
                        Content = m.Content,
                        SenderId = m.SenderId.ToString(),
                        Timestamp = m.CreatedAt
                    })
                .ToList()
        };
    }

    private static string CreateCursor(DateTimeOffset createdAt, Guid id)
    {
        // The "o" format safely handles the absolute offset tracking properties
        string rawStr = $"{createdAt:o}|{id}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(rawStr));
    }

    private static (DateTimeOffset CreatedAt, Guid Id) ParseCursor(string cursorToken)
    {
        try
        {
            string decodedStr = Encoding.UTF8.GetString(Convert.FromBase64String(cursorToken));
            string[] parts = decodedStr.Split('|');

            // Parse directly back into a clean DateTimeOffset instance
            DateTimeOffset createdAt =
                DateTimeOffset.Parse(parts[0], CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);
            Guid id = Guid.Parse(parts[1]);

            return (createdAt, id);
        }
        catch (Exception)
        {
            throw new BadHttpRequestException("Invalid pagination cursor token format provided.");
        }
    }
}

public sealed class GetMessagesRequest
{
    [RouteParam]
    public Guid RoomId { get; set; }

    [QueryParam]
    public string? Cursor { get; set; }

    [QueryParam, DefaultValue(50)]
    public int Limit { get; set; }
}