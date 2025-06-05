using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using GraduationAPI_EPOSHBOOKING.DTO;
using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
#pragma warning disable // tắt cảnh báo để code sạch hơn


namespace GraduationAPI_EPOSHBOOKING.Controllers.Partner
{
    [ApiController]
    [Route("api/v1/partner/room")]
    public class PartnerRoomController : Controller
    {
        private readonly IRoomRepository reponsitory;
        private readonly IConfiguration configuration;
        public PartnerRoomController(IRoomRepository roomRepository, IConfiguration configuration)
        {
            this.reponsitory = roomRepository;
            this.configuration = configuration;
        }



        [HttpDelete("delete-room")]
        public IActionResult DeleteRoom([FromQuery] int roomID)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "partner":
                        var response = reponsitory.DeleteRoom(roomID);
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

        [HttpPost("add-room")]
        public IActionResult AddRoom([FromForm] AddRoomDTO addRoomModel)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "partner":
                        var services = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ServiceTypeDTO>>(addRoomModel.Services);
                        var specialPrices = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SpecialPrice>>(addRoomModel.SpecialPrices);
                        var response = reponsitory.AddRoom(
                            addRoomModel.HotelID,
                            addRoomModel.Room,
                            specialPrices,
                            addRoomModel.Images,
                            services
                        );

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

        [HttpPut("update-room")]
        public IActionResult UpdateRoom([FromForm] UpdateRoomDTO updateRoomModel,[FromForm]String? urlImage)
        {


            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var user = Ultils.Utils.GetUserInfoFromToken(token, configuration);
            try
            {
                switch (user.Role.Name.ToLower())
                {
                    case "partner":
                        var services = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ServiceTypeDTO>>(updateRoomModel.Services);
                        var specialPrice = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SpecialPrice>>(updateRoomModel.specialPrice);
    
                        var response = reponsitory.UpdateRoom(
                            updateRoomModel.RoomID,
                            updateRoomModel.Room,
                            specialPrice,
                            urlImage,
                            updateRoomModel.Images,
                            services);
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
