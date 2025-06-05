using GraduationAPI_EPOSHBOOKING.Model;

namespace GraduationAPI_EPOSHBOOKING.IRepository
{
    public interface IFeedbackRepository
    {
        public ResponseMessage CreateFeedBack(int BookingID,FeedBack feedBack, IFormFile Image);
        public ResponseMessage GetAllFeedbackHotel(int hotelID);
      
    }
}
 