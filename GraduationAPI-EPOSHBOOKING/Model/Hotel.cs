using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("Hotel")]
    public class Hotel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HotelID { get; set; }
        public String MainImage { get; set; }
        public String Name { get; set; }
        public int OpenedIn { get; set; }
        public String Description { get; set; }

        public int HotelStandar { get; set; }
        public String? isRegister { get; set; }

        public bool Status { get; set;}

        [ForeignKey("AccountID")]
        public Account Account { get; set; }

        [ForeignKey("AddressID")]
        public HotelAddress? HotelAddress { get; set; }

        public ICollection<HotelImage>? HotelImages { get; set; }
        public ICollection<HotelService>? HotelServices { get; set; }
        public ICollection<FeedBack>? feedBacks { get; set; }
        public ICollection<Room>? rooms { get; set; }



    }
}
