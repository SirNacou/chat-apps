namespace Server.Domain;

public class Message
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public static Message Create(Guid roomId, Guid senderId, string content)
    {
        return new Message(roomId, senderId, content);
    }

    private Message(Guid roomId, Guid senderId, string content)
    {
        Id = Guid.CreateVersion7();
        RoomId = roomId;
        SenderId = senderId;
        Content = content;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}