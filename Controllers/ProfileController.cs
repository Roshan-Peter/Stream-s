// Controllers/ProfileController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController(AppDbContext db) : ControllerBase
{
    // GET api/profile/{userId}
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetProfile(Guid userId)
    {
        var user = await db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        return Ok(new
        {
            id        = user.Id,
            firstName = user.FirstName,
            lastName  = user.LastName,
            username  = user.Username,
            email     = user.Email,
            isOnline  = user.IsOnline,
        });
    }

    // PUT api/profile/{userId}
    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateProfile(
        Guid userId, [FromBody] UpdateProfileRequest req)
    {
        var user = await db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        // Check username uniqueness if changed
        if (req.Username != user.Username)
        {
            var taken = await db.Users
                .AnyAsync(u => u.Username == req.Username && u.Id != userId);
            if (taken)
                return Conflict(new { message = "Username is already taken." });
        }

        user.FirstName = req.FirstName.Trim();
        user.LastName  = req.LastName.Trim();
        user.Username  = req.Username.Trim();

        await db.SaveChangesAsync();

        return Ok(new
        {
            id        = user.Id,
            firstName = user.FirstName,
            lastName  = user.LastName,
            username  = user.Username,
            email     = user.Email,
        });
    }
}

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string Username
);