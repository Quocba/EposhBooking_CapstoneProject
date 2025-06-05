using GraduationAPI_EPOSHBOOKING.IRepository;
using Microsoft.AspNetCore.Mvc;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Controllers.Admin
{
    [ApiController]
    [Route("api/v1/admin/booking")]
    public class AdminBookingController : Controller
    {
        private readonly IBookingRepository repository;
        private readonly IConfiguration configuration;
        public AdminBookingController(IBookingRepository repository,IConfiguration configuration) { 
          
            this.configuration = configuration;
            this.repository = repository;
        }


        [HttpGet("export-all-bookings-total-revenue")]
        public IActionResult ExportAllBookings()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
   
                        var response = repository.ExportAllBookings();

            if (response.Success)
            {
                return File((byte[])response.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"AllBookings.xlsx");
            }
            else
            {
                return StatusCode(response.StatusCode, response);
            }  
        }

        [HttpGet("export-revenues")]
        public IActionResult ExportRevenues()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            var response = repository.ExportRevenues();
            if (response.Success)
            {
                return File((byte[])response.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Revenues.xlsx");
            }
            else
            {
                return StatusCode(response.StatusCode, response);
            }
        }


        [HttpGet("analysis-revenue")]
        public IActionResult AnalysisRevenueBookingSystem()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = repository.AnalysisRevenueBookingSystem();
                        return StatusCode(response.StatusCode, response);
                    default: return Unauthorized();
                }
            }catch (Exception ex)
            {
                return Unauthorized() ;
            }
        }

        [HttpGet("count-booking-system")]
        public IActionResult CountBookingSystem()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = repository.CountBookingSystem();
                        return StatusCode(response.StatusCode, response);
                    default: return Unauthorized();
                }
            }catch (Exception ex)
            {
                return Unauthorized();
            }

        }

        [HttpGet("get-top-booking")]
        public IActionResult Top5Booking()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = repository.Top5Booking();
                        return StatusCode(response.StatusCode, response);
                    default: return Unauthorized();
                }   
            }catch (Exception ex)
            {
                return Unauthorized();
            }

        }
    }
}
