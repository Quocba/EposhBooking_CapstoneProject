using GraduationAPI_EPOSHBOOKING.Model;
using Microsoft.AspNetCore.Mvc;

namespace GraduationAPI_EPOSHBOOKING.IRepository
{
    public interface IVoucherRepository
    {
        public ResponseMessage GetAllVouchers();
        public ResponseMessage GetVoucherById(int voucherId);
        public ResponseMessage GetVouchersByAccountId(int accountId);
        public ResponseMessage ReceiveVoucher(int voucherId,int accountID);
        public ResponseMessage CreateVoucher(Voucher voucher, IFormFile voucherImage);
        public ResponseMessage DeleteVoucher(int voucherId);
        public ResponseMessage UpdateVoucher(int voucherID, Voucher voucher, IFormFile image);
        public ResponseMessage SearchVoucherName(String  voucherName);
    }
}
