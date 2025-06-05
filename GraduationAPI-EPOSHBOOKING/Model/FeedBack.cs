using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("FeedBack")]
    public class FeedBack
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FeedBackID { get; set; }

        public int Rating { get; set; }
        public String? Image { get; set; }
        public String Description { get; set; }
        public String? Status { get; set; }

        [ForeignKey("AccountID")]
        public Account? Account { get; set; }

        [ForeignKey("BookingID")]
        public Booking? Booking { get; set; }   

        [ForeignKey("HotelID")]
        public Hotel? Hotel { get; set; }

       

    }
}
