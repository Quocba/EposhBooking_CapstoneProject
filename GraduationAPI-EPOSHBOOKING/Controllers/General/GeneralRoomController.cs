using GraduationAPI_EPOSHBOOKING.DTO;
using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using Microsoft.AspNetCore.Mvc;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Controllers.Guest
{
    [ApiController]
    [Route("api/v1/room")]
    public class GeneralRoomController : Controller
    {
        private readonly IRoomRepository reponsitory;
        public GeneralRoomController(IRoomRepository roomRepository) { 
        
            this.reponsitory = roomRepository;
        }
    
        [HttpGet("get-room-by-id")]
        public IActionResult GetRoomDetail([FromQuery]int roomID)
        {
            var response = reponsitory.GetRoomDetail(roomID);
            return StatusCode(response.StatusCode, response);   
        }

        [HttpGet("get-all-room")]
        public IActionResult GetAllRoom()
        {
            var resposne = reponsitory.GetAllRoom();
            return StatusCode(resposne.StatusCode, resposne);
        }
       
        [HttpGet("get-hotel-room")]
        public IActionResult GetRoomByHotel([FromQuery]int hotelID)
        {
            var response = reponsitory.GetRoomByHotel( hotelID );
            return StatusCode(response.StatusCode, response);
        }


    }
}
