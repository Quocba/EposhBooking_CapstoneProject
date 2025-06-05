using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("Profile")]
    public class Profile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProfileID { get; set; }
        [MaxLength(32)]
        public String fullName { get; set; }
        public DateTime? BirthDay { get; set; }
        [MaxLength(10)]
        public String? Gender { get; set; }
        public String? Address { get; set;}
        
        public String? Avatar { get; set; }

    }
}
