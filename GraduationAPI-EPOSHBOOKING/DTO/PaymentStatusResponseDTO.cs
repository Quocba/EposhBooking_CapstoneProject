namespace GraduationAPI_EPOSHBOOKING.DTO
{
    public class PaymentStatusResponseDTO
    {
        public string BookingID { get; set; }
        public string Status { get; set; } // ví dụ: "Success", "Failure"
        public string Message { get; set; } // Thông báo bổ sung nếu có
    }
}
