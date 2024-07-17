using MongoDB.Bson.Serialization.Attributes;

namespace AceBank.Models
{
    public class StaffDetail
    {
        [BsonRequired]
        [BsonId]
        public int EmployeeId { get; set; }

        [BsonRequired]
        public string FirstName { get; set; } = string.Empty;

        [BsonRequired]
        public string LastName { get; set; } = string.Empty;

        [BsonRequired]
        public string Role { get; set; } = string.Empty;
        public StaffLogin LogInCredentials { get; set; } = new StaffLogin();

    }
}
