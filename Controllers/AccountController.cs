using AceBank.Models;
using AceBank.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AceBank.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IService _service;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IService service, ILogger<AccountController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("Sign Up")]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp([FromBody] SignUpDetail signUpDetail)
        {
            if (signUpDetail == null)
            {
                _logger.LogWarning("SignUp detail is null.");
                return BadRequest("Invalid sign up details.");
            }

            try
            {
                var result = await _service.signUp(signUpDetail);
                if (!string.IsNullOrEmpty(result))
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Sign up failed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sign up.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("Get Account Detail")]
        public async Task<IActionResult> GetAccountDetail([FromQuery] long accountNumber, [FromQuery] string username)
        {
            if (accountNumber <= 0 || string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("Invalid account number or username.");
                return BadRequest("Invalid account number or username.");
            }

            try
            {
                var result = await _service.getAccountDetail(accountNumber, username);
                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return NotFound("Account not found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account details.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("User Log In")]
        [AllowAnonymous]
        public async Task<IActionResult> LogIn([FromQuery] UserLogin userLogin)
        {
            if (userLogin == null || userLogin.AccountNumber <= 0 || string.IsNullOrEmpty(userLogin.Password))
            {
                _logger.LogWarning("Invalid login details.");
                return BadRequest("Invalid login details.");
            }

            try
            {
                var result = await _service.logIn(userLogin);
                if (!string.IsNullOrEmpty(result))
                {
                    return Ok(result);
                }
                else
                {
                    return Unauthorized("Login failed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
