using GraduationAPI_EPOSHBOOKING.IRepository;
using Microsoft.AspNetCore.Mvc;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Controllers.Partner
{
    [ApiController]
    [Route("api/v1/partner/booking")]
    public class PartnerBookingController : Controller
    {
        private readonly IBookingRepository repository;
        private readonly IConfiguration configuration;
        public PartnerBookingController(IBookingRepository repository,IConfiguration configuration)
        {
            this.repository = repository;
            this.configuration = configuration;
        }

        [HttpPut("change-wait-for-payment")]
        public IActionResult ChangeStatusWaitForPayment([FromForm] int bookingID)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ","");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch(user.Role.Name.ToLower())
                {
                    case "partner":
                        var response = repository.ChangeStatusWaitForPayment(bookingID);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }catch (Exception ex)
            {
                return Unauthorized();
            }
        }

        [HttpGet("analysis-revenue-hotel")]
        public IActionResult AnalysisRevenueBookingHotel([FromQuery] int hotelID)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "partner":
                        var response = repository.AnalysisRevenueBookingHotel(hotelID);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();

                }
            }catch (Exception ex)
            {
                return Unauthorized();
            }

        }

        [HttpPut("change-complete")]
        public IActionResult ChangeStatusComplete([FromForm] int bookingID)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "partner":
                        var response = repository.ChangeStatusComplete(bookingID);
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

        [HttpGet("count-booking-hotel")]
        public IActionResult CountBookingHotel([FromQuery] int hotelID)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch(user.Role.Name.ToLower())
                {
                    case "partner":
                        var response = repository.CountBookingHotel(hotelID);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }catch (Exception ex)
            {
                return Unauthorized();
            }
  
        }
        //Có vẽ rồi
        [HttpGet("get-booking-hotel")]
        public IActionResult GetBookingByHotel([FromQuery] int hotelID)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "partner":
                        var response = repository.GetBookingByHotel(hotelID);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }catch (Exception ex)
            {
                return Unauthorized();
            }

        }


        [HttpGet("export-bookings-and-total-revenue-by-hotelID")]
        public IActionResult ExportBookingbyHotelID([FromQuery] int hotelID)
        {


            var response = repository.ExportBookingbyHotelID(hotelID);

            if (response.Success)
            {
                return File((byte[])response.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Bookings_HotelID_{hotelID}.xlsx");
            }
            else
            {
                return StatusCode(response.StatusCode, response);
            }

        }
    }
}
