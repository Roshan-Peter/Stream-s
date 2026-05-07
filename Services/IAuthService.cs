// Services/AuthService.cs
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using ChatApp.API.Models;
using Microsoft.EntityFrameworkCore;
using Stream.Schema;

namespace ChatApp.API.Services;






public class AuthService(AppDbContext db, ILogger<AuthService> logger)
{
    // ── Send OTP ──────────────────────────────────────────────────────────────

    public async Task<string> SendOtpAsync(string email)
    {
        // Invalidate any existing unused OTPs for this email
        var existing = await db.OtpRecords
            .Where(o => o.Email == email && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var old in existing)
            old.IsUsed = true;

        // Generate and save new OTP
        var code = GenerateOtp();
        db.OtpRecords.Add(new OtpRecord
        {
            Email     = email,
            Code      = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
        });

        await db.SaveChangesAsync();

        // Log to terminal instead of sending email
        LogOtp(email, code);

        return code;
    }

    // ── Verify OTP + auto register/login ─────────────────────────────────────

    public async Task<AuthResult> VerifyOtpAsync(string email, string code)
    {
        var otp = await db.OtpRecords
            .Where(o =>
                o.Email     == email       &&
                o.Code      == code        &&
                !o.IsUsed                  &&
                o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.ExpiresAt)
            .FirstOrDefaultAsync();

        if (otp is null)
            return new AuthResult(false, "Invalid or expired OTP.", null);

        // Mark OTP as used
        otp.IsUsed = true;

        // Find or create user
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        bool isNewUser = user is null;

        if (isNewUser)
        {
            user = new Users
            {
                Email      = email,
                FirstName  = string.Empty,
                LastName   = string.Empty,
                Username   = await GenerateUniqueUsernameAsync(email),
                IsOnline   = true,
                LastSeenAt = DateTime.UtcNow,
            };
            db.Users.Add(user);
        }
        else
        {
            user!.IsOnline  = true;
            user.LastSeenAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        logger.LogInformation("User {Email} {Status} successfully",
            email, isNewUser ? "registered" : "logged in");

        return new AuthResult(true, isNewUser ? "Registered" : "LoggedIn", user);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string> GenerateUniqueUsernameAsync(string email)
    {
        var base_ = email.Split('@')[0]
            .ToLower()
            .Replace(".", "_")
            .Replace("+", "_");

        var username = $"@{base_}";
        int suffix   = 1;

        while (await db.Users.AnyAsync(u => u.Username == username))
            username = $"@{base_}{suffix++}";

        return username;
    }

    public string GenerateOtp() =>
        RandomNumberGenerator.GetInt32(100000, 999999).ToString();

    private void LogOtp(string email, string code)
    {
        logger.LogInformation("╔══════════════════════════════╗");
        logger.LogInformation("║         OTP CODE             ║");
        logger.LogInformation("║  Email : {Email}", email);
        logger.LogInformation("║  Code  : {Code}              ║", code);
        logger.LogInformation("║  Expires in 5 minutes        ║");
        logger.LogInformation("╚══════════════════════════════╝");
    }
}

// ── Result DTO ────────────────────────────────────────────────────────────────

public record AuthResult(bool Success, string Message, Users? User);

// public class AuthService(AppDbContext db)
// {
//     // ── Send OTP ──────────────────────────────────────────────────────────────

//     public async Task<string> SendOtpAsync(string email)
//     {
//         // Invalidate any existing unused OTPs for this email
//         var existing = await db.OtpRecords
//             .Where(o => o.Email == email && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
//             .ToListAsync();

//         foreach (var old in existing)
//             old.IsUsed = true;

//         // Generate and save new OTP
//         var code = GenerateOtp();
//         db.OtpRecords.Add(new OtpRecord
//         {
//             Email     = email,
//             Code      = code,
//             ExpiresAt = DateTime.UtcNow.AddMinutes(5),
//         });

//         await db.SaveChangesAsync();
//         await SendEmailAsync(email, code);

//         return code; // return for dev/testing; remove in production
//     }

//     // ── Verify OTP + auto register/login ─────────────────────────────────────

//     public async Task<AuthResult> VerifyOtpAsync(string email, string code)
//     {
//         var otp = await db.OtpRecords
//             .Where(o =>
//                 o.Email     == email       &&
//                 o.Code      == code        &&
//                 !o.IsUsed                  &&
//                 o.ExpiresAt > DateTime.UtcNow)
//             .OrderByDescending(o => o.ExpiresAt)
//             .FirstOrDefaultAsync();

//         if (otp is null)
//             return new AuthResult(false, "Invalid or expired OTP.", null);

//         // Mark OTP as used
//         otp.IsUsed = true;

//         // Find or create user
//         var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
//         bool isNewUser = user is null;

//         if (isNewUser)
//         {
//             user = new Users
//             {
//                 Email     = email,
//                 FirstName = string.Empty,
//                 LastName  = string.Empty,
//                 Username  = await GenerateUniqueUsernameAsync(email),
//                 IsOnline  = true,
//                 LastSeenAt = DateTime.UtcNow,
//             };
//             db.Users.Add(user);
//         }
//         else
//         {
//             user!.IsOnline   = true;
//             user.LastSeenAt  = DateTime.UtcNow;
//         }

//         await db.SaveChangesAsync();

//         return new AuthResult(true, isNewUser ? "Registered" : "LoggedIn", user);
//     }

//     // ── Helpers ───────────────────────────────────────────────────────────────

//     private async Task<string> GenerateUniqueUsernameAsync(string email)
//     {
//         var base_ = email.Split('@')[0]
//             .ToLower()
//             .Replace(".", "_")
//             .Replace("+", "_");

//         var username = $"@{base_}";
//         int suffix   = 1;

//         while (await db.Users.AnyAsync(u => u.Username == username))
//             username = $"@{base_}{suffix++}";

//         return username;
//     }

//     public string GenerateOtp()
//     {
//         return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
//     }

//     private async Task SendEmailAsync(string email, string otp)
//     {
//         string senderEmail = Environment.GetEnvironmentVariable("SMTP_EMAIL")
//             ?? throw new Exception("SMTP_EMAIL not set");
//         string pass = Environment.GetEnvironmentVariable("SMTP_PASSWORD")
//             ?? throw new Exception("SMTP_PASSWORD not set");
//         string host = Environment.GetEnvironmentVariable("SMTP_HOST")
//             ?? throw new Exception("SMTP_HOST not set");
//         int port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")
//             ?? throw new Exception("SMTP_PORT not set"));

//         using var smtp = new SmtpClient(host)
//         {
//             Port        = port,
//             Credentials = new NetworkCredential(senderEmail, pass),
//             EnableSsl   = true,
//         };

//         var mail = new MailMessage
//         {
//             From       = new MailAddress(senderEmail),
//             Subject    = "Your verification code",
//             IsBodyHtml = true,
//             Body       = $@"
//                 <div style='font-family:sans-serif;max-width:400px;margin:auto'>
//                   <h2 style='color:#4F46E5'>Verification Code</h2>
//                   <p>Use the code below to sign in. It expires in <b>5 minutes</b>.</p>
//                   <div style='font-size:36px;font-weight:bold;letter-spacing:8px;
//                               color:#111;background:#f4f4f5;padding:20px;
//                               border-radius:8px;text-align:center'>
//                     {otp}
//                   </div>
//                   <p style='color:#888;font-size:12px;margin-top:16px'>
//                     If you didn't request this, ignore this email.
//                   </p>
//                 </div>",
//         };

//         mail.To.Add(email);
//         await smtp.SendMailAsync(mail);
//     }
// }


