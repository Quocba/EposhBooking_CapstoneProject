using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using GraduationAPI_EPOSHBOOKING.Repository;
using Microsoft.AspNetCore.Mvc;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Controllers.Guest
{
    [Route("api/v1/blog")]
    [ApiController]
    public class GeneralBlogController : ControllerBase
    {
        private readonly IBlogRepository _blogRepository;

        public GeneralBlogController(IBlogRepository blogRepository)
        {
            _blogRepository = blogRepository;
        }

        [HttpGet("get-all-blog")]
        public IActionResult GetAllBlogs()
        {
            var response = _blogRepository.GetAllBlogs();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-blog-details")]
        public IActionResult GetBlogDetailById([FromQuery] int blogId)
        {
            var response = _blogRepository.GetBlogDetailById(blogId);
            return StatusCode(response.StatusCode, response);
        }
    
      
        
     
       
    }
}
