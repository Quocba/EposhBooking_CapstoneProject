using GraduationAPI_EPOSHBOOKING.IRepository;
using Microsoft.AspNetCore.Mvc;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Controllers.Admin
{
    [ApiController]
    [Route("api/v1/admin/reportFeedback")]
    public class AdminReportFeedbackController : Controller
    {
        private readonly IReportFeedbackRepository repository;
        private readonly IConfiguration configuration;
        public AdminReportFeedbackController(IReportFeedbackRepository repository, IConfiguration configuration)
        {
            this.repository = repository;
            this.configuration = configuration;
        }

        [HttpGet("get-all-report")]
        public IActionResult GetAllReport()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch(user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = repository.GetAllReportFeedBack();
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }catch (Exception ex)
            {
                return Unauthorized();
            }
        }

        [HttpPut("confirm-report")]
        public IActionResult ConfirmReport([FromQuery] int reportID)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = repository.ConfirmReport(reportID);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }catch (Exception ex)
            {
                return Unauthorized();
            }

        }

        [HttpPut("reject-report")]
        public IActionResult RejectReport([FromForm] int reportID, [FromForm] String ReasonReject)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = repository.RejectReport(reportID, ReasonReject);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }catch (Exception ex)
            {
                return Unauthorized() ;
            }

        }
    }
}
