using MongoDB.Bson.Serialization.Attributes;

namespace AceBank.Models
{
    public class UserLogin
    {
        [BsonRequired]
        [BsonId]
        public string UserName { get; set; } = string.Empty;

        [BsonRequired]
        public long AccountNumber { get; set; }

        [BsonRequired]
        public string Password { get; set; } = string.Empty;
    }
}
