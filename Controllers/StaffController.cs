using AceBank.Models;
using AceBank.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AceBank.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class StaffController : ControllerBase
    {
        private readonly IService _service;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StaffController> _logger;

        public StaffController(IService service, IConfiguration configuration, ILogger<StaffController> logger)
        {
            _service = service;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("StaffLogIn")]
        [AllowAnonymous]
        public async Task<IActionResult> StaffLogIn([FromQuery] StaffLogin stafflogin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var res = await _service.staffLogIn(stafflogin);
                if (res != null)
                {
                    return Ok(res);
                }
                else
                {
                    return NotFound("Staff login failed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during staff login.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("CreditByStaff")]
        public async Task<IActionResult> CreditByStaff([FromBody] StaffDetail staffdetail, [FromQuery] double amount, [FromQuery] long accountnumber)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var res = await _service.creditByStaff(staffdetail, amount, accountnumber);
                if (res != 0)
                {
                    return Ok(res);
                }
                else
                {
                    return BadRequest("Credit operation failed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during credit operation.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("DebitByStaff")]
        public async Task<IActionResult> DebitByStaff([FromBody] StaffDetail staffdetail, [FromQuery] double amount, [FromQuery] long accountnumber)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var res = await _service.debitByStaff(staffdetail, amount, accountnumber);
                if (res != 0)
                {
                    return Ok(res);
                }
                else
                {
                    return BadRequest("Debit operation failed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during debit operation.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("DeleteByStaff")]
        public async Task<IActionResult> DeleteAccount([FromBody] StaffDetail staffdetail, [FromQuery] string key, [FromQuery] long accountnumber, [FromQuery] string username)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (key == _configuration.GetValue<string>("DeleteKey:Key"))
                {
                    var res = await _service.deleteAccount(staffdetail, accountnumber, username);
                    return Ok(res);
                }
                else
                {
                    return Unauthorized("Invalid key.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during account deletion.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
