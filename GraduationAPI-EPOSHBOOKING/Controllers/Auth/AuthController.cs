using Azure.Core;
using GraduationAPI_EPOSHBOOKING.DTO;
using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using GraduationAPI_EPOSHBOOKING.Repository;
using GraduationAPI_EPOSHBOOKING.Ultils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Controllers.Auth
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : Controller
    {

        private readonly IAccountRepository repository;
        private readonly IConfiguration configuration;
        public AuthController(IAccountRepository repository, IConfiguration configuration)
        {
            this.repository = repository;
            this.configuration = configuration;
        }
        [HttpPost("register-singletance")]
        public IActionResult RegisterSingleTance(RegisterDTO registerDTO)
        {
            var response = repository.RegisterSigletance(registerDTO);
            return StatusCode(response.StatusCode,response);
        }

        [HttpPost("partner-register")]
        public IActionResult RegisterPartnerAccount([FromForm] Account account, [FromForm] String fullName)
        {
            var response = repository.RegisterPartnerAccount(account, fullName);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("login-phone")]
        public IActionResult LoginPhone([FromForm] String phone)
        {
            var response = repository.LoginWithNumberPhone(phone);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("active-account")]
        public IActionResult ActiveAccount([FromForm] String email)
        {
            var response = repository.ActiveAccount(email);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("register-customer")]
        public IActionResult Register([FromBody] RegisterDTO register)
        {
            var response = repository.Register(register.Email, register.Password, register.FullName, register.Phone);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                if (user.Role.Name.ToLower().Equals("customer") || user.Role.Name.ToLower().Equals("partner"))
                {
                    var response = repository.ChangePassword(request.AccountId, request.OldPassword, request.NewPassword);
                    return StatusCode(response.StatusCode, response);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                return Unauthorized();
            }

        }

        [HttpPost("send-mail")]
        public IActionResult SendOTPForgot([FromBody] String email)
        {
            var response = Utils.sendMail(email);
            return Ok(response);
        }

        [HttpPut("update-new-password")]
        public IActionResult UpdateNewPassword([FromForm] String newPassword, [FromForm] String email)
        {
            var response = repository.UpdateNewPassword(email, newPassword);
            return StatusCode(response.StatusCode, response);
        }

        //[HttpPut("update-profile")]
        //public async Task<IActionResult> UpdateProfileByAccount([FromForm] String? email, [FromForm] String? phone, [FromForm] Profile? profile, [FromForm] IFormFile? Avatar)
        //{
        //    var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        //    var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
        //    try
        //    {
        //        if (user.Role.Name.ToLower().Equals("customer") || user.Role.Name.ToLower().Equals("partner"))
        //        {
        //            var response = await repository.UpdateProfileByAccount(user.AccountID, email, phone, profile, Avatar);
        //            return StatusCode(response.StatusCode, response);
        //        }
        //        else
        //        {
        //            return Unauthorized();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Unauthorized();
        //    }

        //}
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            var response = repository.Login(login.text, login.Password);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-time-server")]
        public IActionResult GetTime()
        {
            var time = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
            return Ok(time);
        }

        [HttpGet("get-profile-by-account")]
        public IActionResult GetProfileByAccountId()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                if (user.Role.Name.ToLower().Equals("customer") || user.Role.Name.ToLower().Equals("partner"))
                {
                    var response = repository.GetProfileByAccountId(user.AccountID);
                    return StatusCode(response.StatusCode, response);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                return Unauthorized();
            }

        }

        [HttpPost("google-login")]
        public IActionResult GoogleLogin([FromForm] String email, [FromForm] String userName, [FromForm] String avartar)
        {
            var reponse = repository.GoogleLogin(email, userName, avartar);
            return StatusCode(reponse.StatusCode, reponse);
        }

        [HttpPut("update-email")]
        public IActionResult UpdateEmail([FromForm] int accountID, [FromForm] String email)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                if (user.Role.Name.ToLower().Equals("customer") || user.Role.Name.ToLower().Equals("partner"))
                {
                    var response = repository.UpdateEmail(accountID, email);
                    return StatusCode(response.StatusCode, response);
                }
                else
                {
                    return Unauthorized();
                }

            }
            catch (Exception ex)
            {
                return Unauthorized();
            }
        }

        [HttpPut("update-phone")]
        public IActionResult UpdatePone([FromForm] int accountID, [FromForm] String phone)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                if (user.Role.Name.ToLower().Equals("customer") || user.Role.Name.ToLower().Equals("partner"))
                {
                    var response = repository.UpdatePhone(accountID, phone);
                    return StatusCode(response.StatusCode, response);
                }
                else
                {
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
