namespace GraduationAPI_EPOSHBOOKING.DTO
{
    public class GetHotelDTO 
    {
        public int HotelID { get; set; }
        public String MainImage { get; set; }
        public String Name { get; set; }
        public int OpenedIn { get; set; }
        public String Description { get; set; }

        public int HotelStandar { get; set; }
        public String? isRegister { get; set; }

        public bool Status { get; set; }
        public String OwnerName { get; set; }
        public String Address { get; set; }


    }
}
