namespace Server.Domain;

public sealed class Room
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public RoomType Type { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    private readonly List<RoomMember> _members;
    public IReadOnlyCollection<RoomMember> Members => _members.AsReadOnly();

    private readonly List<Message> _messages = new();
    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    public static Room CreateGroupRoom(string name, string creatorId)
    {
        var room = new Room(name, RoomType.Group);
        room.AddMembers([creatorId]);

        return room;
    }

    public static Room CreateDirectMessageRoom(string creatorId, string recipientId)
    {
        var room = new Room("", RoomType.DirectMessage);
        room.AddMembers([creatorId, recipientId]);
        return room;
    }

    public void AddMembers(IReadOnlyCollection<string> userIds)
    {
        if (userIds.Count == 0)
            return;

        if (_members.Any(m => userIds.Contains(m.UserId)))
        {
            throw new InvalidOperationException("User is already a member of the room.");
        }

        _members.AddRange(
            userIds.Select(userId => new RoomMember { UserId = userId, RoomId = Id })
        );
    }

    private Room(string name, RoomType type)
    {
        Id = Guid.CreateVersion7();
        Name = name;
        Type = type;
        CreatedAt = DateTimeOffset.UtcNow;
        _members = [];
    }
}

public enum RoomType
{
    Group,
    DirectMessage
}