using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using Microsoft.AspNetCore.Mvc;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Controllers.Admin
{
    [ApiController]
    [Route("api/v1/admin/account")]
    public class AdminAccountController : Controller
    {
        private readonly IAccountRepository repository;
        private readonly IConfiguration configuration;
        public AdminAccountController(IAccountRepository _repository, IConfiguration configuration)
        {
            repository = _repository;
            this.configuration = configuration;
        }
        [HttpGet("get-all-sigletance")]
        public IActionResult GetAllSigngleTance()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ","");
            var getUser = Ultils.Utils.GetUserInfoFromToken(token,configuration);
            switch (getUser.Role.Name.ToLower())
            {
                case "admin":
                    var reponse = repository.GetAllAccountSigletacne();
                    return StatusCode(reponse.StatusCode, reponse);
                default:
                    return Unauthorized();
                    
            }
        }
        [HttpGet("get-token")]
        public IActionResult GetUser([FromForm]String token)
        {
            var result = Ultils.Utils.GetUserInfoFromToken(token,configuration);
            return Ok(result);
        }
        [HttpGet("get-all")]
        public IActionResult GetAllAccount()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ","");
            var user = Ultils.Utils.GetUserInfoFromToken(token,configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = repository.GetAllAccount();
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }catch (Exception ex)
            {
                return Unauthorized();
            }

        }

        [HttpPut("blocked-account")]
        public IActionResult BlockedAccount([FromForm] int accountId, [FromForm] String reasonBlock)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = repository.BlockedAccount(accountId, reasonBlock);
                        return StatusCode(response.StatusCode, response);
                    default:
                        return Unauthorized();
                }
            }catch (Exception ex)
            {
                return Unauthorized() ;
            }
                
        }

        [HttpGet("filter-account")]
        public IActionResult FilterAccountByStatus([FromQuery] bool isActive)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = repository.FilterAccountByStatus(isActive);
                        return StatusCode(response.StatusCode, response);
                    default : return Unauthorized();
                }
            }catch (Exception ex)
            {
                return Unauthorized();
            }

        }

        [HttpGet("searchByName")]
        public IActionResult SearchAccountByName([FromQuery] string name)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "admin":
                        var response = repository.SearchAccountByName(name);
                        return StatusCode(response.StatusCode, response);
                    default : 
                        return Unauthorized();
                }
            }catch(Exception ex) {
             
                return Unauthorized();
            }
   
        }

    }
}
