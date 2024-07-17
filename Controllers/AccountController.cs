using AceBank.Models;
using AceBank.Service;
using Amazon.Auth.AccessControlPolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace AceBank.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[Authorize]
    
    public class AccountController : ControllerBase
    {
        private readonly IService _service;
        public AccountController(IService service)
        {
            _service = service;
        }

        [HttpPost("Sign Up")]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp([FromBody] SignUpDetail signupdetail)
        {
            var res =await _service.signUp(signupdetail);
            if(res != null)
            {
                return Ok(res);
            }
            else
            {
                return BadRequest(res);
            }
        }

        [HttpGet("Get Account Detail")]
        public async Task<IActionResult> GetAccountDetail([FromQuery] long accountnumber, string username)
        {
            var res = await _service.getAccountDetail(accountnumber, username);
            if(res != null)
            {
                return Ok(res);
            }
            else
            {
                return NotFound(res);
            }
        }

        [HttpGet("User Log In")]
        [AllowAnonymous]
        public async Task<IActionResult> LogIn([FromQuery] UserLogin userlogin)
        {
            var res = await _service.logIn(userlogin);
            if (res != null)
            {
                return Ok(res);
            }
            else
            {
                return NotFound(res);
            }
        }

    }
}
