namespace GraduationAPI_EPOSHBOOKING.DTO
{
    public class PaymentRequestDTO
    {
        public int BookingID { get; set; }
        public int TotalPrice { get; set; }
        public string Description { get; set; }
        public string SuccessUrl { get; set; }
        public string FailureUrl { get; set; }
    }
}
