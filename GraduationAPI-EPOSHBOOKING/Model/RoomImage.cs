using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("RoomImage")]
    public class RoomImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ImageID { get; set; }
        public String Image { get; set; }

        [ForeignKey("RoomID")]
        public Room Room { get; set; }
    }
}
