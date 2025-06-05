namespace GraduationAPI_EPOSHBOOKING.DTO
{
    public class CreateBookingDTO
    {
        public int AccountID { get; set; }
        public int? VoucherID { get; set; }
        public int RoomID { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public double TotalPrice { get; set; }
        public double TaxesPrice { get; set; }
        public int NumberOfGuest { get; set; }
        public int NumberOfRoom { get; set; }



    }
}
