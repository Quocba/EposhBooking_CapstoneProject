using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationAPI_EPOSHBOOKING.Model
{
    [Table("MyVoucher")]
    public class MyVoucher
    {
        [Key]
        public int AccountID { get; set; }
        public Account Account { get; set; }

        [Key]
        public int VoucherID { get; set; }
        public Voucher Voucher { get; set; }

        public bool IsVoucher { get; set; }
    }
}
