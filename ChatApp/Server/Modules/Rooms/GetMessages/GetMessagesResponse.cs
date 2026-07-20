namespace Server.Modules.Rooms.GetMessages;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
public sealed class GetMessagesResponse
{
    public List<MessageDto> Messages { get; set; }
    public string? NextCursor { get; set; }
    public bool HasNextPage { get; set; }

    public sealed class MessageDto
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public string SenderId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
