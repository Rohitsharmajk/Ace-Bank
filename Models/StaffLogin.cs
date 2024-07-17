using MongoDB.Bson.Serialization.Attributes;

namespace AceBank.Models
{
    public class StaffLogin
    {
        [BsonRequired]
        [BsonId]
        public int EmployeeId { get; set; }

        [BsonRequired]
        public string Password { get; set; } = string.Empty;
    }
}
