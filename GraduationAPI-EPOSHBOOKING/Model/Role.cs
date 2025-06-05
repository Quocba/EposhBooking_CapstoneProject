using GraduationAPI_EPOSHBOOKING.Model.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("Role")]
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleID { get; set; }
        [MaxLength(10)]
        public String Name { get; set; }

        public RoleEnum roleEnum => (RoleEnum)RoleID;
    }
}
