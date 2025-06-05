using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using Microsoft.AspNetCore.Mvc;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Controllers.Partner
{
    [ApiController]
    [Route("api/v1/partner/reportFeedback")]
    public class PartnerReportFeedbackController : Controller
    {
        private readonly IReportFeedbackRepository repository;
        private readonly IFeedbackRepository feedbackRepository;
        private readonly IConfiguration configuration;
        public PartnerReportFeedbackController(IReportFeedbackRepository repository, IConfiguration configuration,IFeedbackRepository feedbackRepository)
        {
            this.repository = repository;
            this.configuration = configuration;
            this.feedbackRepository = feedbackRepository;
        }

        [HttpPost("create-report")]
        public IActionResult CreateReportFeedback([FromForm] int feedbackId, [FromForm] String ReporterEmail, [FromForm] String ReasonReport)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "partner":
                        var response = repository.CreateReportFeedback(feedbackId, ReporterEmail, ReasonReport);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            } catch (Exception ex)
            {
                return Unauthorized();
            }

        }

        [HttpGet("get-hotel-feedback")]
        public IActionResult GetAllHotelFeedback([FromQuery]int hotelID)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "partner":
                        var response = feedbackRepository.GetAllFeedbackHotel(hotelID);
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
