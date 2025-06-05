using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("Blogs")]
    public class Blog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BlogID { get; set; }
        public String Title { get; set; }
        public String Description { get; set; }
        public String Location { get; set; }
        public DateTime PublishDate { get; set; }
        public String? Status { get; set; }
        
        public String? ReasonReject { get; set; }
        [ForeignKey("AccountID")]
        public Account? Account { get; set; }

        public ICollection<CommentBlog>? Comment { get; set; }
        public ICollection<BlogImage>? BlogImage { get; set; }
    }
}
