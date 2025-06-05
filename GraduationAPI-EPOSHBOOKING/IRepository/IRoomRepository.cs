using GraduationAPI_EPOSHBOOKING.DTO;
using GraduationAPI_EPOSHBOOKING.Model;

namespace GraduationAPI_EPOSHBOOKING.IRepository
{
    public interface IRoomRepository
    {
        ResponseMessage GetRoomDetail(int roomID);
        ResponseMessage GetAllRoom();// Không dùng tới.
        ResponseMessage DeleteRoom(int roomID);
        ResponseMessage AddRoom(int hotelID,Room room,List<SpecialPrice>specialPrices, List<IFormFile>images,List<ServiceTypeDTO>services);
        ResponseMessage UpdateRoom(int roomID,Room room,List<SpecialPrice> specialPrices, String imageUrl, List<IFormFile>image,List<ServiceTypeDTO>services);
        ResponseMessage GetRoomByHotel(int hotelID);
    }
}
