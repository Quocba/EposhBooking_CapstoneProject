using GraduationAPI_EPOSHBOOKING.IRepository;
using Microsoft.AspNetCore.Mvc;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Controllers.Admin
{
    [ApiController]
    [Route("api/v1/admin/blog")]
    public class AdminBlogController : Controller
    {

        private readonly IBlogRepository _blogRepository;
        private readonly IConfiguration configuration;
        public AdminBlogController(IBlogRepository blogRepository,IConfiguration configuration) { 
            this.configuration = configuration;
            this._blogRepository = blogRepository;
        }

        [HttpPut("confirm-blog")]
        public IActionResult ConfirmBlog([FromQuery] int blogId)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = _blogRepository.ConfirmBlog(blogId);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
              
            }catch(Exception ex)
            {
                return Unauthorized();
            }
           
        }
        [HttpPut("reject-blog")]
        public IActionResult RejectBlog([FromForm] int blogId, [FromForm] string reasonReject)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = _blogRepository.RejectBlog(blogId, reasonReject);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }catch(Exception ex)
            {
                return Unauthorized();
            }
       
        }
    }
}
