using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("ReportFeedBack")]
    public class ReportFeedBack
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReportID { get; set; }

        public String ReporterEmail { get; set; }

        public String? ReasonReport { get;set; }
        public String Status { get; set; }
        [ForeignKey("FeedBackID")]
        public FeedBack FeedBack { get; set; }

    }
}
