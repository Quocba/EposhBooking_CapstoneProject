using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("CommentBlog")]
    public class CommentBlog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommentID { get; set; }
        public String Description { get; set; }
        public DateTime DateComment { get; set; }
        [ForeignKey("BlogID")]
        public Blog blog { get; set; }
        [ForeignKey("AccountID")]
        public Account account { get; set; }
       
    }
}
