using AceBank.Models;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AceBank.Service
{
    public class Service : IService
    {
        private readonly MongoClient _client;
        private readonly IMongoCollection<SignUpDetail> _signupcollection;
        private readonly IMongoCollection<AccountDetail> _accountcollection;
        private readonly IMongoCollection<StaffLogin> _staffcollection;
        private readonly IConfiguration _configuration;
        public Service(IConfiguration configuration)
        {
            _configuration = configuration;
            _client = new MongoClient("mongodb+srv://rohitbralace08:qwertyu%21op1Qq@acecluster.z8rbymm.mongodb.net/?retryWrites=true&w=majority&appName=AceCluster");
            var _database = _client.GetDatabase("AceBank");
            _signupcollection = _database.GetCollection<SignUpDetail>("SignUpCollection");
            _accountcollection = _database.GetCollection<AccountDetail>("AccountCollection");
            _staffcollection = _database.GetCollection<StaffLogin>("StaffCollection");

        }
        public async Task<string> signUp(SignUpDetail signupdetail)
        {
            try
            {
                if (signupdetail.UserDetail == null || signupdetail == null)
                {
                    return "Null Data Error";
                }
                await _signupcollection.InsertOneAsync(signupdetail);
                byte[] buffer = new byte[8];
                Random random = new Random();
                random.NextBytes(buffer);
                long result = BitConverter.ToInt64(buffer, 0);
                result = Math.Abs(result);

                if (result !=0)
                {
                    AccountDetail accountdetail = new AccountDetail();
                    accountdetail.AccountNumber = result;
                    accountdetail.signUpDetail = signupdetail;
                    accountdetail.AccountBalance = 0;
                    accountdetail.CreatedOn = DateOnly.FromDateTime(DateTime.Now);
                    accountdetail.Updated = DateOnly.FromDateTime(DateTime.Now);
                    accountdetail.UserDetail = signupdetail.UserDetail;
                    accountdetail.Type = AccountDetail.AccountType.Saving;
                    accountdetail.UserName = signupdetail.UserName;

                    await _accountcollection.InsertOneAsync(accountdetail);
                    return "Account Created Successfully! We'll mail your account details";
                }
                return "Error Occurred";
            }

            catch
            {
                throw new Exception("System Error");
            }
        }

        public async Task<AccountDetail> getAccountDetail(long accountnumber, string username)
        {
            try
            {
                AccountDetail res = new AccountDetail();
                var filter = Builders<AccountDetail>.Filter.Eq(u => u.AccountNumber, accountnumber);
                res = await _accountcollection.Find(filter).FirstOrDefaultAsync();
                if (res!=null && res.signUpDetail.UserName == username)
                {
                    res.signUpDetail.UserName = "Restricted";
                    res.signUpDetail.Password = "Restricted";
                    return res;
                }
                return new AccountDetail();
            }

            catch
            {
                return new AccountDetail();

            }
            
        }

        public async Task<string> logIn(UserLogin userlogin)
        {
            string role = "users";
            var filter = Builders<AccountDetail>.Filter.Eq(u => u.AccountNumber, userlogin.AccountNumber);
            AccountDetail res = await _accountcollection.Find(filter).FirstOrDefaultAsync();
            if (res != null && res.signUpDetail.Password == userlogin.Password && res.signUpDetail.UserName == res.UserName)
            {
                string token = generateToken(role);
                return token;
            }
            return "Error Occurred";
        }

        public async Task<string> staffLogIn(StaffLogin stafflogin)
        {
            string role = "staffs";
            var filter = Builders<StaffLogin>.Filter.Eq(u => u.EmployeeId, stafflogin.EmployeeId);
            var user = await _staffcollection.Find(filter).FirstOrDefaultAsync();
            if(user != null && user.EmployeeId == stafflogin.EmployeeId && user.Password == stafflogin.Password)
            {
                string token = generateToken(role);
                return token;
            }
            return "Error Occurred";
        }

        private string generateToken(string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("Ace Bank is Lit, Boht Acha! Now Credit Cards also Available");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, "Admin"),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(20),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = "MyIssuer",
                Audience = "MyAudience"
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

        public async Task<double> creditByStaff(StaffDetail staffdetail, double amount, long accountnumber)
        {
            var filter = Builders<AccountDetail>.Filter.Eq(u=>u.AccountNumber, accountnumber);
            var res = await _accountcollection.Find(filter).FirstOrDefaultAsync();
            if(res != null)
            {
                res.AccountBalance = res.AccountBalance+amount;
                var result = await _accountcollection.ReplaceOneAsync(x => x.AccountNumber == accountnumber, res);
                if(result.IsAcknowledged)
                {
                    res.Updated = DateOnly.FromDateTime(DateTime.Now);
                    return res.AccountBalance;
                }
                throw new Exception("Update not Successful");
            }
            throw new Exception("Null Data Error");
        }

        public async Task<double> debitByStaff(StaffDetail staffdetail, double amount, long accountnumber)
        {
            try
            {
                var filter = Builders<AccountDetail>.Filter.Eq(u => u.AccountNumber, accountnumber);
                var res = await _accountcollection.Find(filter).FirstOrDefaultAsync();
                if (res != null)
                {
                    if (res.AccountBalance != 0 || res.AccountBalance > amount || amount > 0)
                    {
                        res.AccountBalance = res.AccountBalance - amount;
                        var result = await _accountcollection.ReplaceOneAsync(x => x.AccountNumber == accountnumber, res);
                        if (result.IsAcknowledged)
                        {
                            res.Updated = DateOnly.FromDateTime(DateTime.Now);
                            return res.AccountBalance;
                        }
                        throw new Exception("Update not successful");
                    }
                    else
                    {
                        throw new Exception("Insufficent Funds");
                    }
                }
                throw new Exception("Null Data Error");
            }
            catch (Exception ex)
            {
                throw new Exception("Error Occurred");
            }
        }

    }
}
