using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AceBank.Models
{
    public class UserDetail
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public int Age { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public enum IdType
        {
            Adhaar,
            PanId
        }


        [Required]
        public string IdNumber { get; set; } = string.Empty;
        public bool KycCompleted = false;

    }
}
