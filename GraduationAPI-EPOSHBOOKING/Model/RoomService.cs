using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("RoomService")]
    public class RoomService
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomServiceID { get; set; }
        public String Type { get; set; }
        [ForeignKey("RoomID")]  
        public Room Room { get; set; }
        public ICollection<RoomSubService> RoomSubServices { get; set; }
    }
}   
