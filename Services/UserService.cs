using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stream.Schema;

public class UserService
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<Users> _hasher;

    public UserService(AppDbContext context, PasswordHasher<Users> hasher)
    {
        _context = context;
        _hasher = hasher;
    }



    public async Task<Users?> LoginAsync(string email, string password)
    {
        var user = new Users();

        return user;
    }


    public async Task UpdateUsernameAsync(Guid userId, string newUsername)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        user.Username = newUsername;

        await _context.SaveChangesAsync();
    }
}