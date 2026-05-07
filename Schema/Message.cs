// Models/Message.cs
using System.ComponentModel.DataAnnotations;
using Stream.Schema;

namespace ChatApp.API.Models;

public enum MessageType
{
    Text  = 0,
    Image = 1,
    File  = 2,
    Audio = 3
}

public class Message
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;

    [Required]
    public Guid SenderId { get; set; }
    public Users Sender { get; set; } = null!;

    [Required, MaxLength(4000)]
    public string Text { get; set; } = string.Empty;

    public MessageType   Type   { get; set; } = MessageType.Text;
    public MessageStatus Status { get; set; } = MessageStatus.Sent;

    // For file/image/audio messages
    [MaxLength(500)]
    public string? MediaUrl { get; set; }

    // Reply threading
    public Guid? ReplyToMessageId { get; set; }
    public Message? ReplyToMessage { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }

    // ── Navigation ──────────────────────────────────────────────────────────

    public ICollection<MessageReadReceipt> ReadReceipts { get; set; } = [];
}

// ── Per-user read receipts ────────────────────────────────────────────────────

public class MessageReadReceipt
{
    public Guid MessageId { get; set; }
    public Message Message { get; set; } = null!;

    public Guid UserId { get; set; }
    public Users User { get; set; } = null!;

    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
}