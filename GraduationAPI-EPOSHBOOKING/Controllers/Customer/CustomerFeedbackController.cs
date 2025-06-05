using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using Microsoft.AspNetCore.Mvc;

namespace GraduationAPI_EPOSHBOOKING.Controllers.Customer
{
    [ApiController]
    [Route("api/v1/customer/feedback")]
    public class CustomerFeedbackController : Controller
    {
        private readonly IFeedbackRepository repository;
        private readonly IConfiguration configuration;
        public CustomerFeedbackController(IFeedbackRepository repository, IConfiguration configuration)
        {
            this.repository = repository;
            this.configuration = configuration;
        }

        [HttpPost("create-feedback")]
        public IActionResult CreateFeedBack([FromForm] int BookingID, [FromForm] FeedBack newFeedBack, [FromForm] IFormFile? Image)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ","");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch(user.Role.Name.ToLower())
                {
                    case "customer":
                        var response = repository.CreateFeedBack(BookingID, newFeedBack, Image);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }catch (Exception ex)
            {
                return Unauthorized();
            }
 
        }
    }
}
