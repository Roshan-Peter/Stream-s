// Models/Conversation.cs
using System.ComponentModel.DataAnnotations;
using Stream.Schema;

namespace ChatApp.API.Models;

public enum ConversationType
{
    Direct = 0,   // 1-to-1
    Group  = 1    // group chat
}

public class Conversation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public ConversationType Type { get; set; } = ConversationType.Direct;

    // Only used for group chats
    [MaxLength(100)]
    public string? GroupName { get; set; }

    [MaxLength(500)]
    public string? GroupAvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──────────────────────────────────────────────────────────

    public ICollection<ConversationParticipant> Participants { get; set; } = [];

    public ICollection<Message> Messages { get; set; } = [];
}

// ── Join table: users ↔ conversations ────────────────────────────────────────

public class ConversationParticipant
{
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;

    public Guid UserId { get; set; }
    public Users User { get; set; } = null!;

    // When this user joined the conversation
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Last message this user has read (for per-user read receipts)
    public Guid? LastReadMessageId { get; set; }
    public Message? LastReadMessage { get; set; }

    // Soft mute/archive per user
    public bool IsMuted    { get; set; } = false;
    public bool IsArchived { get; set; } = false;
}