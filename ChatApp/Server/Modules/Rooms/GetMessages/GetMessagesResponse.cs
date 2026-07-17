namespace Server.Modules.Rooms.GetMessages;

public sealed class GetMessagesResponse
{
    public required List<MessageDto> Messages { get; set; }
    public string? NextCursor { get; set; }
    public bool HasNextPage { get; set; }

    public sealed class MessageDto
    {
        public required string Id { get; set; }
        public required string Content { get; set; }
        public required string SenderId { get; set; }
        public required DateTimeOffset Timestamp { get; set; }
    }
}