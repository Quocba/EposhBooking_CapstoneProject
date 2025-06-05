using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationAPI_EPOSHBOOKING.Model
{
        [Table("Booking")]
        public class Booking
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int BookingID { get; set; }
            public DateTime CheckInDate { get; set; }
            public DateTime CheckOutDate { get; set; }
            public double TotalPrice { get; set; }
            public double UnitPrice { get; set; }
            public double TaxesPrice { get; set; }
            public int NumberOfRoom { get; set; }
            public int NumberGuest { get; set; }
            public String? ReasonCancle { get; set;}
            public String? Status { get; set; }
        
            [ForeignKey("AccountID")]
            public Account? Account { get; set; }

            [ForeignKey("RoomID")]
            public Room? Room { get; set; }

            [ForeignKey("VoucherID")]
            public Voucher? Voucher { get; set; }
            
            public ICollection<FeedBack>? feedBacks { get; set; }
 
    }
}
