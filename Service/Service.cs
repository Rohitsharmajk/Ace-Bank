using AceBank.Models;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AceBank.Service
{
    public class Service : IService
    {
        private readonly MongoClient _client;
        private readonly IMongoCollection<SignUpDetail> _signupCollection;
        private readonly IMongoCollection<AccountDetail> _accountCollection;
        private readonly IMongoCollection<StaffLogin> _staffCollection;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Service> _logger;

        public Service(IConfiguration configuration, ILogger<Service> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            var database = _client.GetDatabase("AceBank");
            _signupCollection = database.GetCollection<SignUpDetail>("SignUpCollection");
            _accountCollection = database.GetCollection<AccountDetail>("AccountCollection");
            _staffCollection = database.GetCollection<StaffLogin>("StaffCollection");
        }

        public async Task<string> signUp(SignUpDetail signupDetail)
        {
            try
            {
                if (signupDetail?.UserDetail == null)
                {
                    return "Null Data Error";
                }

                await _signupCollection.InsertOneAsync(signupDetail);

                long accountNumber = GenerateUniqueAccountNumber();

                if (accountNumber != 0)
                {
                    var accountDetail = new AccountDetail
                    {
                        AccountNumber = accountNumber,
                        signUpDetail = signupDetail,
                        AccountBalance = 0,
                        CreatedOn = DateOnly.FromDateTime(DateTime.Now),
                        Updated = DateOnly.FromDateTime(DateTime.Now),
                        UserDetail = signupDetail.UserDetail,
                        Type = AccountDetail.AccountType.Saving,
                        UserName = signupDetail.UserName
                    };

                    await _accountCollection.InsertOneAsync(accountDetail);
                    return "Account Created Successfully! We'll mail your account details";
                }

                return "Error Occurred";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sign up");
                throw new Exception("System Error");
            }
        }

        public async Task<AccountDetail> getAccountDetail(long accountNumber, string username)
        {
            try
            {
                var filter = Builders<AccountDetail>.Filter.Eq(u => u.AccountNumber, accountNumber);
                var accountDetail = await _accountCollection.Find(filter).FirstOrDefaultAsync();

                if (accountDetail != null && accountDetail.signUpDetail.UserName == username)
                {
                    accountDetail.signUpDetail.UserName = "Restricted";
                    accountDetail.signUpDetail.Password = "Restricted";
                    return accountDetail;
                }

                return new AccountDetail();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching account details");
                return new AccountDetail();
            }
        }

        public async Task<string> logIn(UserLogin userLogin)
        {
            try
            {
                var filter = Builders<AccountDetail>.Filter.Eq(u => u.AccountNumber, userLogin.AccountNumber);
                var accountDetail = await _accountCollection.Find(filter).FirstOrDefaultAsync();

                if (accountDetail != null &&
                    accountDetail.signUpDetail.Password == userLogin.Password &&
                    accountDetail.signUpDetail.UserName == accountDetail.UserName)
                {
                    return GenerateToken("users");
                }

                return "Error Occurred";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return "Error Occurred";
            }
        }

        public async Task<string> staffLogIn(StaffLogin staffLogin)
        {
            try
            {
                var filter = Builders<StaffLogin>.Filter.Eq(u => u.EmployeeId, staffLogin.EmployeeId);
                var staff = await _staffCollection.Find(filter).FirstOrDefaultAsync();

                if (staff != null && staff.Password == staffLogin.Password)
                {
                    return GenerateToken("staffs");
                }

                return "Error Occurred";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during staff login");
                return "Error Occurred";
            }
        }

        private string GenerateToken(string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, "Admin"),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(20),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<double> creditByStaff(StaffDetail staffDetail, double amount, long accountNumber)
        {
            try
            {
                var filter = Builders<AccountDetail>.Filter.Eq(u => u.AccountNumber, accountNumber);
                var accountDetail = await _accountCollection.Find(filter).FirstOrDefaultAsync();

                if (accountDetail != null)
                {
                    accountDetail.AccountBalance += amount;
                    accountDetail.Updated = DateOnly.FromDateTime(DateTime.Now);

                    var result = await _accountCollection.ReplaceOneAsync(x => x.AccountNumber == accountNumber, accountDetail);
                    if (result.IsAcknowledged)
                    {
                        return accountDetail.AccountBalance;
                    }

                    throw new Exception("Update not Successful");
                }

                throw new Exception("Account not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during credit operation");
                throw new Exception("System Error");
            }
        }

        public async Task<double> debitByStaff(StaffDetail staffDetail, double amount, long accountNumber)
        {
            try
            {
                var filter = Builders<AccountDetail>.Filter.Eq(u => u.AccountNumber, accountNumber);
                var accountDetail = await _accountCollection.Find(filter).FirstOrDefaultAsync();

                if (accountDetail != null)
                {
                    if (accountDetail.AccountBalance >= amount && amount > 0)
                    {
                        accountDetail.AccountBalance -= amount;
                        accountDetail.Updated = DateOnly.FromDateTime(DateTime.Now);

                        var result = await _accountCollection.ReplaceOneAsync(x => x.AccountNumber == accountNumber, accountDetail);
                        if (result.IsAcknowledged)
                        {
                            return accountDetail.AccountBalance;
                        }

                        throw new Exception("Update not successful");
                    }

                    throw new Exception("Insufficient Funds");
                }

                throw new Exception("Account not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during debit operation");
                throw new Exception("System Error");
            }
        }

        public async Task<string> deleteAccount(StaffDetail staffDetail, long accountNumber, string username)
        {
            try
            {
                var accountFilter = Builders<AccountDetail>.Filter.Eq(u => u.AccountNumber, accountNumber);
                var userFilter = Builders<SignUpDetail>.Filter.Eq(u => u.UserName, username);

                var accountResult = await _accountCollection.DeleteOneAsync(accountFilter);
                var signupResult = await _signupCollection.DeleteOneAsync(userFilter);

                if (accountResult.IsAcknowledged && signupResult.IsAcknowledged)
                {
                    return $"Deleted account {accountNumber} successfully.";
                }

                throw new Exception("Error occurred while deleting the account");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during account deletion");
                throw new Exception("Internal Error Occurred");
            }
        }

        private long GenerateUniqueAccountNumber()
        {
            byte[] buffer = new byte[8];
            new Random().NextBytes(buffer);
            long accountNumber = BitConverter.ToInt64(buffer, 0);
            return Math.Abs(accountNumber);
        }
    }
}
