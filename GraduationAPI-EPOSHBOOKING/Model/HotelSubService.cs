using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("HotelSubService")]
    public class HotelSubService
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubServiceID { get; set; }
        public String SubServiceName { get; set; }

        [ForeignKey("ServiceID")]
        public HotelService HotelService { get; set; }

    }
}
