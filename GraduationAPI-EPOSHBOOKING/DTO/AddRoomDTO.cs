using GraduationAPI_EPOSHBOOKING.Model;

namespace GraduationAPI_EPOSHBOOKING.DTO
{
    public class AddRoomDTO
    {
        public int HotelID { get; set; }
        public Room Room { get; set; }
        public string SpecialPrices { get; set; }
        public List<IFormFile> Images { get; set; }
        public string Services { get; set; }
    }
}
