using AceBank.Models;
using AceBank.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AceBank.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class StaffController : ControllerBase
    {

        private readonly IService _service;
        public StaffController(IService service)
        {
            _service = service;
        }

        [HttpGet("Staff Log In")]
        [AllowAnonymous]
        public async Task<IActionResult> StaffLogIn([FromQuery] StaffLogin stafflogin)
        {
            var res = await _service.staffLogIn(stafflogin);
            if (res != null)
            {
                return Ok(res);
            }
            else
            {
                return NotFound(res);
            }
        }

        [HttpPut("Credit by Staff")]
        public async Task<IActionResult> CreditByStaff(StaffDetail staffdetail, double amount, long accountnumber)
        {
            var res = await _service.creditByStaff(staffdetail, amount, accountnumber);
            if (res != 0)
            {
                return Ok(res);
            }
            else
            {
                return BadRequest(res);
            }
        }

        [HttpPut("Debit by Staff")]
        public async Task<IActionResult> DebitByStaff(StaffDetail staffdetail, double amount, long accountnumber)
        {
            var res = await _service.debitByStaff(staffdetail, amount, accountnumber);
            if (res != 0)
            {
                return Ok(res);
            }
            else
            {
                return BadRequest(res);
            }
        }

    }
}
