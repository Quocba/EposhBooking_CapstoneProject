using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("HotelService")]
    public class HotelService
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceID { get; set; }
        public String Type { get; set; }

        [ForeignKey("HotelID")]
        public Hotel Hotel { get; set; }
        public ICollection<HotelSubService> HotelSubServices { get; set; }
    }
}
