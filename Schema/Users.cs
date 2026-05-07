using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using ChatApp.API.Models;

namespace Stream.Schema
{
    
    public class Users : BaseEntity
    {

        [Required]
        [MaxLength(100)]
        public string FirstName {get; set;} = string.Empty;

        [MaxLength(100), NotNull]
        public string? LastName {get; set;} = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email {get; set;} = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Username {get; set;} = string.Empty;

         [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        public bool IsOnline { get; set; } = false;

        public DateTime? LastSeenAt { get; set; }


        // ── Navigation ──────────────────────────────────────────────────────────

        // Conversations this user is part of
        public ICollection<ConversationParticipant> Participants { get; set; } = [];

        // Messages sent by this user
        public ICollection<Message> SentMessages { get; set; } = [];

        // Computed
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public string Initials =>
            $"{(FirstName.Length > 0 ? FirstName[0] : ' ')}{(LastName.Length > 0 ? LastName[0] : ' ')}";
    }



}