namespace GraduationAPI_EPOSHBOOKING.DTO
{
    public class ChangePasswordRequest
    {
        public int AccountId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
