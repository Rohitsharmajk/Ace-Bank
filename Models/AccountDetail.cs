using MongoDB.Bson.Serialization.Attributes;

namespace AceBank.Models
{
    public class AccountDetail
    {
        [BsonId]
        [BsonRequired]
        public long AccountNumber { get; set; }

        [BsonRequired]
        public string UserName { get; set; } = string.Empty;

        [BsonRequired]
        public UserDetail? UserDetail { get; set; }

        [BsonRequired]
        public double AccountBalance { get; set; }

        [BsonRequired]
        public SignUpDetail signUpDetail { get; set; }

        [BsonRequired]
        public DateOnly CreatedOn { get; set; }

        public DateOnly Updated {  get; set; }

        public enum AccountType
        {
            Saving,
            Current,
            Salary
        }

        [BsonRequired]
        public AccountType Type { get; set; }

        [BsonRequired]
        public bool KycCompleted { get; set; } = false;
    }
}
