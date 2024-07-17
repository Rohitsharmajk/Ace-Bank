using AceBank.Models;

namespace AceBank.Service
{
    public interface IService
    {
        Task<string> signUp(SignUpDetail signupdetail);
        Task<string> logIn(UserLogin userlogin);
        Task<string> staffLogIn(StaffLogin stafflogin);
        Task<AccountDetail> getAccountDetail(long accountnumber, string username);
        Task<double> creditByStaff(StaffDetail staffdetail, double amount, long accountnumber);
        Task<double> debitByStaff(StaffDetail staffdetail, double amount, long accountnumber);
    }
}
