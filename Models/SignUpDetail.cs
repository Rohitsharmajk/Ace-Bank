using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AceBank.Models
{
    public class SignUpDetail
    {
        [Required]
        [BsonId]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        public UserDetail? UserDetail { get; set; }
    }
}
