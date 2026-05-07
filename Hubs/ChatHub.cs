// Hubs/ChatHub.cs
using ChatApp.API.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Hubs;

public class ChatHub(AppDbContext db) : Hub
{
    // userId → connectionId map (in-memory, use Redis for multi-server)
    private static readonly Dictionary<string, string> _onlineUsers = [];

    // ── Connection ────────────────────────────────────────────────────────────

    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
        if (string.IsNullOrEmpty(userId))
        {
            Context.Abort();
            return;
        }

        _onlineUsers[userId] = Context.ConnectionId;

        // Mark user online in DB
        var user = await db.Users.FindAsync(Guid.Parse(userId));
        if (user is not null)
        {
            user.IsOnline    = true;
            user.LastSeenAt  = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        // Join all conversation groups this user is part of
        var conversationIds = await db.ConversationParticipants
            .Where(cp => cp.UserId == Guid.Parse(userId))
            .Select(cp => cp.ConversationId.ToString())
            .ToListAsync();

        foreach (var convId in conversationIds)
            await Groups.AddToGroupAsync(Context.ConnectionId, convId);

        // Notify contacts that this user is online
        await Clients.Others.SendAsync("UserOnline", userId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _onlineUsers
            .FirstOrDefault(x => x.Value == Context.ConnectionId).Key;

        if (userId is not null)
        {
            _onlineUsers.Remove(userId);

            var user = await db.Users.FindAsync(Guid.Parse(userId));
            if (user is not null)
            {
                user.IsOnline   = false;
                user.LastSeenAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }

            await Clients.Others.SendAsync("UserOffline", userId, DateTime.UtcNow);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ── Messaging ─────────────────────────────────────────────────────────────

    public async Task SendMessage(SendMessageDto dto)
    {
        var message = new Message
        {
            Id             = Guid.NewGuid(),
            ConversationId = dto.ConversationId,
            SenderId       = dto.SenderId,
            Text           = dto.Text,
            Type           = MessageType.Text,
            Status         = MessageStatus.Sent,
            CreatedAt      = DateTime.UtcNow,
        };

        db.Messages.Add(message);

        // Update conversation timestamp
        var conversation = await db.Conversations.FindAsync(dto.ConversationId);
        if (conversation is not null)
            conversation.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        var response = new MessageDto
        {
            Id             = message.Id,
            ConversationId = message.ConversationId,
            SenderId       = message.SenderId,
            Text           = message.Text,
            Status         = message.Status,
            CreatedAt      = message.CreatedAt,
        };

        // Broadcast to everyone in the conversation group
        await Clients
            .Group(dto.ConversationId.ToString())
            .SendAsync("ReceiveMessage", response);
    }

    // ── Typing indicator ──────────────────────────────────────────────────────

    public async Task StartTyping(Guid conversationId, Guid userId)
    {
        await Clients
            .OthersInGroup(conversationId.ToString())
            .SendAsync("UserTyping", conversationId, userId);
    }

    public async Task StopTyping(Guid conversationId, Guid userId)
    {
        await Clients
            .OthersInGroup(conversationId.ToString())
            .SendAsync("UserStoppedTyping", conversationId, userId);
    }

    // ── Read receipts ─────────────────────────────────────────────────────────

    public async Task MarkAsRead(Guid conversationId, Guid userId, Guid lastMessageId)
    {
        // Upsert read receipt
        var existing = await db.ConversationParticipants
            .FirstOrDefaultAsync(cp =>
                cp.ConversationId == conversationId &&
                cp.UserId         == userId);

        if (existing is not null)
        {
            existing.LastReadMessageId = lastMessageId;
            await db.SaveChangesAsync();
        }

        // Update message statuses to Read
        var unreadMessages = await db.Messages
            .Where(m =>
                m.ConversationId == conversationId &&
                m.SenderId       != userId         &&
                m.Status         != MessageStatus.Read)
            .ToListAsync();

        foreach (var msg in unreadMessages)
            msg.Status = MessageStatus.Read;

        await db.SaveChangesAsync();

        // Notify the sender their messages were read
        await Clients
            .Group(conversationId.ToString())
            .SendAsync("MessagesRead", conversationId, userId, lastMessageId);
    }

    // ── Online status query ───────────────────────────────────────────────────

    public Task<List<string>> GetOnlineUsers()
    {
        return Task.FromResult(_onlineUsers.Keys.ToList());
    }
}