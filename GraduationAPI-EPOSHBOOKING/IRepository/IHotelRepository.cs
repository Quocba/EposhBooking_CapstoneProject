
using GraduationAPI_EPOSHBOOKING.DTO;
using GraduationAPI_EPOSHBOOKING.Model;

namespace GraduationAPI_EPOSHBOOKING.IRepository
{
    public interface IHotelRepository
    {
        public ResponseMessage GetAllHotel();
        public ResponseMessage GetHotelByCity(String city);
        public ResponseMessage GetHotelByID (int hotelID);
        public ResponseMessage GetHotelByPrice(String city, double minPrice, double maxPrice);
        public ResponseMessage GetByHotelStandar(int hotelStandar);
        public ResponseMessage GetByService(List<String> services);
        public ResponseMessage GetServiceByHotelID(int hotelID);
        public ResponseMessage GetGalleriesByHotelID(int hotelID);
        public ResponseMessage SearchHotel(String name);
        public ResponseMessage HotelRegistration(HotelRegistrationDTO registration, List<ServiceTypeDTO> Services);
        public ResponseMessage UpdateBasicInformation(int hotelID,string hotelName,
                                                     int openedIn,
                                                     string description,
                                                     IFormFile mainImage);
        public ResponseMessage UpdateHotelService(int hotelID,List<ServiceTypeDTO>services);
        public ResponseMessage AddHotelImage(int hotelId,String title, IFormFile image);
        public ResponseMessage DeleteHotelImages(int imageID);
        public ResponseMessage GetAllHotelInfomation();
        public ResponseMessage FilterHotelByStatus(bool Status);
        public ResponseMessage BlockedHotel(int hotelID, String reaseonBlock);
        public ResponseMessage ConfirmRegistration(int hotelID);
        public ResponseMessage RejectRegistration(int hotelID,String reasonReject);
        public ResponseMessage SearchHotelByName(String hotelName);
        public ResponseMessage GetAllHotelWaitForApproval();
        public ResponseMessage AnalyzeHotelStandar();
        public ResponseMessage GetGuestReviewByHotel(int hotelID);
        public ResponseMessage GetBasicInformation(int hotelID);
        public ResponseMessage GetAddressByHotel(int hotelID);
        public ResponseMessage UpdateAddressByHotel(int hotelID, HotelAddress newAddress);
        public ResponseMessage GetAllCity();
        public ResponseMessage SearchHotelCity(String city);

    }

 
}
