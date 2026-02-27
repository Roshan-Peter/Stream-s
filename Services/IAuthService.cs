using Microsoft.AspNetCore.Identity;
using Stream.Schema;

public interface IAuthService
{
    string HashPassword(Users user, string password);
    bool VerifyPassword(Users user, string providedPassword);
}

public class AuthService : IAuthService
{
    private readonly PasswordHasher<Users> _hasher = new PasswordHasher<Users>();

    public string HashPassword(Users user, string password)
    {
        // Returns a base64 string containing: 
        // format marker + hashing algorithm + salt + subkey
        return _hasher.HashPassword(user, password);
    }

    public bool VerifyPassword(Users user, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, providedPassword);
        
        return result switch
        {
            PasswordVerificationResult.Success => true,
            PasswordVerificationResult.SuccessRehashNeeded => true, 
            _ => false
        };
    }
}