namespace Server.Domain;

public class Room
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsDirectMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public List<RoomMember> Members { get; set; }

    public static Room Create(string name, bool isDirectMessage)
    {
        return new Room(name, isDirectMessage);
    }

    private Room(string name, bool isDirectMessage)
    {
        Id = Guid.CreateVersion7();
        Name = name;
        IsDirectMessage = isDirectMessage;
        CreatedAt = DateTimeOffset.UtcNow;
        Members = [];
    }
}