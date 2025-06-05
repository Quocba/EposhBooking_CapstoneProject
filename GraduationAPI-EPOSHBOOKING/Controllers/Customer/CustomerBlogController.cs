using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using Microsoft.AspNetCore.Mvc;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Controllers.Customer
{
    [ApiController]
    [Route("api/v1/customer/blog")]
    public class CustomerBlogController : Controller
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IConfiguration configuration;
        public CustomerBlogController(IBlogRepository blogRepository, IConfiguration configuration)
        {
            _blogRepository = blogRepository;
            this.configuration = configuration;
        }

        [HttpGet("get-blog-by-account")]
        public IActionResult GetBlogsByAccountId([FromQuery] int accountId)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "customer":
                        var response = _blogRepository.GetBlogsByAccountId(accountId);
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

        [HttpPost("create-blog")]
        public IActionResult CreateBlog([FromForm] Blog blog, [FromForm] int accountID, [FromForm] List<IFormFile> image)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToString())
                {
                    case "customer":
                        var response = _blogRepository.CreateBlog(blog, accountID, image);
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

        [HttpDelete("delete-blog")]
        public IActionResult DeleteBlog([FromQuery] int blogId)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "customer":
                        var response = _blogRepository.DeleteBlog(blogId);
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

        [HttpPost("comment-blog")]
        public IActionResult CommentBlog([FromForm] int blogId, [FromForm] int accountId, [FromForm] string description)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToString())
                {
                    case "customer":
                        var response = _blogRepository.CommentBlog(blogId, accountId, description);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }
            catch(Exception ex)
            {
                return Unauthorized();
            }
        }

        [HttpGet("filter-blog-with-status")]
        public IActionResult FilterBlogwithStatus([FromQuery] String status)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "customer":
                        var response = _blogRepository.FilterBlogwithStatus(status);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }
            catch(Exception ex)
            {
                return Unauthorized();
            }
           

        }

    }
}
