// Controllers/ConversationsController.cs
using ChatApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/[controller]")]
public class ConversationsController(AppDbContext db) : ControllerBase
{
    // POST api/conversations/direct
    [HttpPost("direct")]
    public async Task<IActionResult> GetOrCreate([FromBody] DirectConversationRequest req)
    {
        // Check if a direct conversation already exists between these two users
        var existing = await db.ConversationParticipants
            .Where(cp => cp.UserId == req.UserAId)
            .Select(cp => cp.ConversationId)
            .Intersect(
                db.ConversationParticipants
                    .Where(cp => cp.UserId == req.UserBId)
                    .Select(cp => cp.ConversationId)
            )
            .Join(
                db.Conversations.Where(c => c.Type == ConversationType.Direct),
                id => id,
                c => c.Id,
                (id, c) => c.Id
            )
            .FirstOrDefaultAsync();

        if (existing != default)
            return Ok(new { conversationId = existing });

        // Create new direct conversation
        var conversation = new Conversation
        {
            Type      = ConversationType.Direct,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Conversations.Add(conversation);

        db.ConversationParticipants.AddRange(
            new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId         = req.UserAId,
                JoinedAt       = DateTime.UtcNow,
            },
            new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId         = req.UserBId,
                JoinedAt       = DateTime.UtcNow,
            }
        );

        await db.SaveChangesAsync();

        // Add the new user to the SignalR group via Hub context
        return Ok(new { conversationId = conversation.Id });
    }
}

public record DirectConversationRequest(Guid UserAId, Guid UserBId);