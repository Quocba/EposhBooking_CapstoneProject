using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("BlogImage")]
    public class BlogImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ImageID { get; set; }
        public String Image { get; set; }

        [ForeignKey("BlogID")]
        public Blog Blog { get; set; }
    }
}
