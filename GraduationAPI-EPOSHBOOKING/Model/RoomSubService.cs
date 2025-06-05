using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("RoomSubService")]
    public class RoomSubService
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubServiceID { get; set; }
        public String SubName { get; set; }

        [ForeignKey("RoomServiceID")]
        public RoomService RoomService { get; set; }

    }
}
