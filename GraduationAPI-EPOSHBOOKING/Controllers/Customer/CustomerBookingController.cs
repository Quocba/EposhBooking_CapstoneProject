using GraduationAPI_EPOSHBOOKING.DTO;
using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Controllers.Customer
{
    [ApiController]
    [Route("api/v1/customer/booking")]
    public class CustomerBookingController : Controller
    {
        private readonly IBookingRepository repository;
        private readonly IConfiguration configuration;
        public CustomerBookingController(IBookingRepository repository, IConfiguration configuration)
        {
            this.repository = repository;
            this.configuration = configuration;
        }

        [HttpGet("get-by-accountID")]
        public IActionResult GetBookingByAccount([FromQuery] int accountID)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "customer":
                        var response = repository.GetBookingByAccount(accountID);
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

        [HttpPut("cancle-booking")]
        public IActionResult CancleBooking([FromForm] int bookingID, [FromForm] string Reason)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "customer":
                        var response = repository.CancleBooking(bookingID, Reason);
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

        [HttpPost("create-booking")]
        public IActionResult CreateBooking([FromForm] CreateBookingDTO? createBookingDTO)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "customer":
                        var response = repository.CreateBooking(createBookingDTO);

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

        [HttpPost("check-room-price")]
        public IActionResult CheckRoomPrice([FromForm] int roomID, [FromForm] DateTime CheckInDate, [FromForm] DateTime CheckOutDate)
        {
            double roomPrice = repository.CheckRoomPrice(roomID, CheckInDate, CheckOutDate);
            return Ok(new { Price = roomPrice });
        }

        [HttpGet("export-bookings-by-accountID")]
        public IActionResult ExportBookingsByAccountID([FromQuery] int accountID)
        {
            var response = repository.ExportBookingsByAccountID(accountID);

            if (response.Success)
            {
                return File((byte[])response.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Bookings_{accountID}.xlsx");
            }
            else
            {
                return StatusCode(response.StatusCode, response);
            }
        }

        [HttpPost("create-booking-online")]
        public IActionResult CreateBookingOnline([FromForm] CreateBookingDTO createBookingDTO)
        {

            var response = repository.CreateBooking(createBookingDTO);
            var bookingData = (dynamic)response.Data;
            string description = $"BID-{bookingData.BookingID}-TP-{bookingData.TotalPrice}";
            var paymentRequest = new PaymentRequestDTO
            {
                BookingID = bookingData.BookingID,
                TotalPrice = (int)bookingData.TotalPrice,
                Description = description,
                SuccessUrl = "https://chatgpt.com/c/55d2ec59-5044-46ad-af65-d40adf9e180e",
                FailureUrl = "https://www.facebook.com/",
            };
            var linkCheckOut = repository.GeneratePaymentLink(paymentRequest);
            return Ok(new { CheckoutUrl = linkCheckOut, response.Data });
        }

        [HttpGet("get-booking-details")]
        public IActionResult GetBookingDetails([FromQuery] int bookingID)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "customer":
                        var response = repository.GetBookingDetails(bookingID);
                        return StatusCode(response.StatusCode, response);
            default:
                        return Unauthorized();
        }
    }
            catch
            {
                return Unauthorized();
}
        }
    }
}
