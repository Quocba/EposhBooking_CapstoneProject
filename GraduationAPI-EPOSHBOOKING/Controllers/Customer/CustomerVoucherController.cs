using GraduationAPI_EPOSHBOOKING.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
#pragma warning disable // tắt cảnh báo để code sạch hơn


namespace GraduationAPI_EPOSHBOOKING.Controllers.Customer
{
    [Route("api/v1/customer/voucher")]
    [ApiController]
    public class CustomerVoucherController : Controller
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IConfiguration configuration;
        public CustomerVoucherController(IVoucherRepository voucherRepository, IConfiguration configuration)
        {
            this._voucherRepository = voucherRepository;
            this.configuration = configuration;
        }

        [HttpGet("get-voucher-by-account")]
        public IActionResult GetVouchersByAccountId([FromQuery] int accountId)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token,configuration);
            try
            {
                switch(user.Role.Name.ToLower())
                {
                    case "customer":
                        var response = _voucherRepository.GetVouchersByAccountId(accountId);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }catch (Exception ex)
            {
                return Unauthorized();
            }

        }

        [HttpPost("receive-voucher")]
        public IActionResult ReceviceVoucher([FromForm] int accountID, [FromForm] int voucherID)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "customer":
                        var response = _voucherRepository.ReceiveVoucher(accountID, voucherID);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                return Unauthorized();
            }

        }
    }
}
