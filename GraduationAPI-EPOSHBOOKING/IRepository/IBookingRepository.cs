using GraduationAPI_EPOSHBOOKING.DTO;
using GraduationAPI_EPOSHBOOKING.Model;

namespace GraduationAPI_EPOSHBOOKING.IRepository
{
    public interface IBookingRepository
    {
        public ResponseMessage GetBookingByAccount(int AccountID);
        public ResponseMessage CancleBooking(int bookingID, String Reason);
        public ResponseMessage CreateBooking(CreateBookingDTO newBooking);
        public ResponseMessage ChangeStatusWaitForPayment(int bookingID);
        public ResponseMessage ChangeStatusComplete(int bookingID);
        public ResponseMessage GetAllBooking();
        public ResponseMessage ExportBookingsByAccountID(int accountID);
        public ResponseMessage ExportAllBookings();
        public ResponseMessage AnalysisRevenueBookingSystem();
        public ResponseMessage AnalysisRevenueBookingHotel(int hotelID);
        public ResponseMessage CountBookingSystem();
        public ResponseMessage CountBookingHotel(int hotelID);
        public ResponseMessage Top5Booking();
        public ResponseMessage GetBookingByHotel(int hotelID);
        public ResponseMessage ExportBookingbyHotelID(int hotelID);
        public ResponseMessage ExportRevenues();
        public double CheckRoomPrice(int roomID, DateTime CheckInDate, DateTime CheckOutDate);

        public ResponseMessage GetBookingDetails(int bookingID);
        Task<String> GeneratePaymentLink(PaymentRequestDTO request);
    }
}
