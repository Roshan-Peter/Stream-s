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


    public async Task<Guid> RegisterAsync(string FirstName, string LastName, string username, string email, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Email == email || u.Username == username))
        {
            throw new InvalidOperationException("Username or Email is already taken.");
        }

        var user = new Users
        {
            FirstName = FirstName,
            LastName = LastName,
            Username = username,
            Email = email
        };

        // 3. Hash Password
        user.PasswordHash = _hasher.HashPassword(user, password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user.Id;
    }

    public async Task<Users?> LoginAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _hasher.HashPassword(user, password);
            await _context.SaveChangesAsync();
        }

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