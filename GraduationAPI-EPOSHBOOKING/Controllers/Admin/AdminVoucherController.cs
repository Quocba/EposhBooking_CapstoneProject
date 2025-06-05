using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using Microsoft.AspNetCore.Mvc;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Controllers.Admin
{
    [Route("api/v1/admin/voucher")]
    [ApiController]
    public class AdminVoucherController : Controller
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IConfiguration configuration;
        public AdminVoucherController(IVoucherRepository voucherRepository, IConfiguration configuration)
        {
            _voucherRepository = voucherRepository;
            this.configuration = configuration;
        }

        [HttpPost("create-voucher")]
        public IActionResult CreateVoucher([FromForm] Voucher voucher, [FromForm] IFormFile image)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch(user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = _voucherRepository.CreateVoucher(voucher, image);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }catch (Exception ex)
            {
                return Unauthorized();
            }

        }

        [HttpDelete("delete-voucher")]
        public IActionResult DeleteVoucher([FromQuery] int voucherID)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = _voucherRepository.DeleteVoucher(voucherID);
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

        [HttpPut("update-voucher")]
        public IActionResult UpdateVoucher([FromForm] int voucherID, [FromForm] Voucher voucher, [FromForm] IFormFile? image)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = _voucherRepository.UpdateVoucher(voucherID, voucher, image);
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

        [HttpGet("search-name")]
        public IActionResult SearchVoucherName([FromQuery] String voucherName)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = _voucherRepository.SearchVoucherName(voucherName);
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
