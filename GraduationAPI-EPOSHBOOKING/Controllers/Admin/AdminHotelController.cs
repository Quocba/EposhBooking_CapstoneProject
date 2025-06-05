using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using Microsoft.AspNetCore.Mvc;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Controllers.Admin
{

    [ApiController]
    [Route("api/v1/admin/hotel")]
    public class AdminHotelController : Controller
    {
        private readonly IHotelRepository repository;
        private readonly IConfiguration configuration;
        public AdminHotelController(IHotelRepository repository, IConfiguration configuration)
        {
            this.repository = repository;
            this.configuration = configuration;
        }

        [HttpGet("get-all-hotel-infomation")]
        public IActionResult GetAllHotelInfomation()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch(user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = repository.GetAllHotelInfomation();
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }catch (Exception ex)
            {
                return Unauthorized();
            }

        }

        [HttpPut("blocked-hotel")]
        public IActionResult BlockedHotel([FromForm] int hotelId,[FromForm] String reaseonBlock)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = repository.BlockedHotel(hotelId, reaseonBlock);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }

            }catch (Exception ex)
            {
                return Unauthorized();
            }

        }

        [HttpPut("confirm-registration")]
        public IActionResult ConfirmRegistration([FromForm] int hotelID)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = repository.ConfirmRegistration(hotelID);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }catch (Exception ex)
            {
                return Unauthorized() ;
            }
  
        }

        [HttpPut("reject-registration")]
        public IActionResult RejectRegistration([FromForm] int hotelID, [FromForm] String reasonReject)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = repository.RejectRegistration(hotelID, reasonReject);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }catch (Exception ex)
            {
                return Unauthorized();
            }

        }
        [HttpGet("searchByName")]
        public IActionResult SearchByName([FromQuery] string hotelName)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = repository.SearchHotelByName(hotelName);
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

        [HttpGet("get-all-registration-form")]
        public IActionResult GetAllHotelWaitForApproval()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = repository.GetAllHotelWaitForApproval();
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

        [HttpGet("analyze-hotelStander")]
        public IActionResult AnalyzeHotelStandar()
        {

            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = repository.AnalyzeHotelStandar();
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
