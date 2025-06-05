using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("Voucher")]
    public class Voucher
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VoucherID { get; set; }
        public String? VoucherImage { get; set; }
        public string VoucherName { get; set;}
        [MaxLength(6)]
        public String Code { get;set;}
        public int QuantityUse { get; set;}
        public double Discount { get; set;}
        public string Description { get; set;}
        public ICollection<MyVoucher>? MyVouchers { get; set; }
    }
}
