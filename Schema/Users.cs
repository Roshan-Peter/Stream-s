using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

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

    }

}