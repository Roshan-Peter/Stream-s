// Hubs/ChatDtos.cs
public class SendMessageDto
{
    public Guid   ConversationId { get; set; }
    public Guid   SenderId       { get; set; }
    public string Text           { get; set; } = string.Empty;
}

public class MessageDto
{
    public Guid          Id             { get; set; }
    public Guid          ConversationId { get; set; }
    public Guid          SenderId       { get; set; }
    public string        Text           { get; set; } = string.Empty;
    public MessageStatus Status         { get; set; }
    public DateTime      CreatedAt      { get; set; }
}