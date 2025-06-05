using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("SpecialPrice")]
    public class SpecialPrice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SpecialPriceID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Double Price { get; set; }
        [ForeignKey("RoomID")]
        public Room Room { get; set; }

    }
}
