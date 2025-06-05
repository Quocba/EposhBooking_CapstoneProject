using GraduationAPI_EPOSHBOOKING.Model;

namespace GraduationAPI_EPOSHBOOKING.DTO
{
    public class UpdateRoomDTO
    {
        public int RoomID { get; set; }
        public Room Room { get; set; }
        public String specialPrice { get; set; }
        public List<IFormFile>? Images { get; set; }
        public string Services { get; set; }
    }
}
