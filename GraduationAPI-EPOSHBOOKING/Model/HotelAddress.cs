using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("HotelAddress")]
    public class HotelAddress
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AddressID { get; set; }
        public String Address { get; set; }
        public String City { get; set; }
        public double latitude { get; set; }
        public double longitude { get;set; }
    }
}
