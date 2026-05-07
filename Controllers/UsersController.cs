// Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(AppDbContext db) : ControllerBase
{
    // GET api/users/{userId}/conversations
    [HttpGet("{userId}/conversations")]
    public async Task<IActionResult> GetConversations(Guid userId)
    {
        var conversations = await db.ConversationParticipants
            .Where(cp => cp.UserId == userId)
            .Include(cp => cp.Conversation)
                .ThenInclude(c => c.Participants)
                    .ThenInclude(p => p.User)
            .Include(cp => cp.Conversation)
                .ThenInclude(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .Select(cp => new
            {
                conversationId = cp.ConversationId,
                // The other participant
                otherUser = cp.Conversation.Participants
                    .Where(p => p.UserId != userId)
                    .Select(p => new
                    {
                        id           = p.User.Id,
                        firstName    = p.User.FirstName,
                        lastName     = p.User.LastName,
                        username     = p.User.Username,
                        isOnline     = p.User.IsOnline,
                        lastSeenAt   = p.User.LastSeenAt,
                    })
                    .FirstOrDefault(),
                lastMessage = cp.Conversation.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => m.Text)
                    .FirstOrDefault() ?? "",
                lastMessageTime = cp.Conversation.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => (DateTime?)m.CreatedAt)
                    .FirstOrDefault(),
                unread = cp.Conversation.Messages
                    .Count(m => m.SenderId != userId && m.Status != MessageStatus.Read),
            })
            .OrderByDescending(x => x.lastMessageTime)
            .ToListAsync();

        var result = conversations
            .Where(c => c.otherUser != null)
            .Select(c => new
            {
                id             = c.otherUser!.id,
                firstName      = c.otherUser.firstName,
                lastName       = c.otherUser.lastName,
                username       = c.otherUser.username,
                isOnline       = c.otherUser.isOnline,
                lastMessage    = c.lastMessage,
                time           = FormatTime(c.lastMessageTime),
                unread         = c.unread,
                conversationId = c.conversationId,
            });

        return Ok(result);
    }

    // GET api/users/search?query=...
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return Ok(new List<object>());

        var users = await db.Users
            .Where(u =>
                u.Username.Contains(query) ||
                u.FirstName.Contains(query) ||
                u.LastName.Contains(query)  ||
                u.Email.Contains(query))
            .Take(20)
            .Select(u => new
            {
                id        = u.Id,
                firstName = u.FirstName,
                lastName  = u.LastName,
                username  = u.Username,
                isOnline  = u.IsOnline,
                lastMessage    = "",
                time           = "",
                unread         = 0,
                conversationId = (Guid?)null,
            })
            .ToListAsync();

        return Ok(users);
    }

    private static string FormatTime(DateTime? dt)
    {
        if (dt is null) return "";
        var now   = DateTime.UtcNow;
        var diff  = now - dt.Value;
        if (diff.TotalMinutes < 1)   return "Just now";
        if (diff.TotalHours   < 24)  return dt.Value.ToString("h:mm tt");
        if (diff.TotalDays    < 2)   return "Yesterday";
        if (diff.TotalDays    < 7)   return dt.Value.ToString("ddd");
        return dt.Value.ToString("dd/MM/yyyy");
    }
}