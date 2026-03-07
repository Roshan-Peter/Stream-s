using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Stream.Schema;


public class AuthService
{

    public async Task<string> SendEmail(string email)
    {
        string senderEmail = Environment.GetEnvironmentVariable("SMTP_EMAIL")?? throw new Exception("SMTP_EMAIL environment variable not set");
        string pass = Environment.GetEnvironmentVariable("SMTP_PASSWORD")?? throw new Exception("SMTP_pass environment variable not set");
        string host = Environment.GetEnvironmentVariable("SMTP_HOST")?? throw new Exception("SMTP_Host environment variable not set");
        int port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")?? throw new Exception("SMTP_Port environment variable not set"));

        var smtpClient = new SmtpClient(host)
        {
            Port = port,
            Credentials = new NetworkCredential(senderEmail, pass),
            EnableSsl = true,
        };

        string otp = GenerateOtp();

        var mail = new MailMessage
        {
            From = new MailAddress(senderEmail),
            Subject = "Authentication Code",
            Body = $@"
                <h2>Email Verification</h2>
                <p>Your One Time Password is:</p>
                <h1>{otp}</h1>
                <p>This code will expire in 5 minutes.</p>
                ",
            IsBodyHtml = true,
        };

        mail.To.Add(email);

        await smtpClient.SendMailAsync(mail);

        return otp;
    }


    public string GenerateOtp()
    {
        int otp = RandomNumberGenerator.GetInt32(100000, 999999);
        return otp.ToString();
    }
}