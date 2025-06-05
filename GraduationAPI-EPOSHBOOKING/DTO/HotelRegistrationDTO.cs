namespace GraduationAPI_EPOSHBOOKING.DTO
{
    public class HotelRegistrationDTO
    {
        public string HotelName { get; set; }
        public int OpenedIn { get; set; }
        public string Description { get; set; }
        public string HotelAddress { get; set; }
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<IFormFile> Images { get; set; }
        public IFormFile MainImage { get; set; }
        public int AccountID { get; set; }
        public string Services { get; set; } // Changed to string to capture JSON
    }
}
