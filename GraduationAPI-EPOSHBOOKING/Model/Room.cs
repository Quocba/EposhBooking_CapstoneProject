using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("Room")]
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomID { get; set; }
        public String TypeOfRoom { get;set; }
        public int NumberCapacity { get; set; }
        public Double Price { get; set; }
        public int Quantity { get; set; }
        public int SizeOfRoom { get; set; }
        public String TypeOfBed { get; set; }
        public int NumberOfBed { get; set; }

        [ForeignKey("HotelID")]
        public Hotel? Hotel { get; set; }

        public ICollection<RoomImage>? RoomImages { get; set; }
        
        public ICollection<RoomService>? RoomService { get; set; }
        public ICollection<SpecialPrice>? SpecialPrice { get; set; }
 
    }
}
