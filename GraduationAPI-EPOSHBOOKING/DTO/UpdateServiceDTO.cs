namespace GraduationAPI_EPOSHBOOKING.DTO
{
    public class UpdateServiceDTO
    {
        public int HotelID { get; set; }
        public List<ServiceTypeDTO> Services { get; set; }
    }
}
