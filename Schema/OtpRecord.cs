// Models/OtpRecord.cs
using System.ComponentModel.DataAnnotations;

namespace ChatApp.API.Models;

public class OtpRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5);

    public bool IsUsed { get; set; } = false;
}