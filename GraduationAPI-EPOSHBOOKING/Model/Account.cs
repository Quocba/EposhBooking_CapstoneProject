using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace GraduationAPI_EPOSHBOOKING.Model
{

    [Table("Account")]
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccountID { get; set; }
        public String? Email { get; set; }
        public String? Password { get; set; }
        [MaxLength(10)] 
        public String? Phone {  get; set; }
        public bool IsActive { get; set; }

        [ForeignKey("RoleID")]
        public Role? Role { get; set; }

        [ForeignKey("ProfileID")]
        public Profile? Profile { get; set; }

        public ICollection<Hotel>? Hotel { get; set; }
        public ICollection<MyVoucher>? MyVouchers { get; set; }
        public ICollection<Blog>? Blogs { get; set; }

    }
}
