using GraduationAPI_EPOSHBOOKING.DataAccess;
using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Repository
{
    public class VoucherRepository : IVoucherRepository

    {
        private readonly DBContext db;
        private readonly IWebHostEnvironment environment;
        public VoucherRepository(DBContext _db, IWebHostEnvironment environment)
        {
            this.db = _db;
            this.environment = environment;
        }

        public ResponseMessage CreateVoucher(Voucher voucher,IFormFile voucherImage)
        {
            var checkAlreadyExistCode = db.voucher.FirstOrDefault(code => code.Code == voucher.Code);
            if (checkAlreadyExistCode == null)
            {
                Voucher createVouvhcer = new Voucher
                {
                    Code = voucher.Code,
                    Description = voucher.Description,
                    Discount = voucher.Discount,
                    QuantityUse = voucher.QuantityUse,
                    VoucherImage = Ultils.Utils.SaveImage(voucherImage,environment),
                    VoucherName = voucher.VoucherName
                };
                db.voucher.Add(createVouvhcer);
                db.SaveChanges();
                return new ResponseMessage {Success = true, Data = createVouvhcer, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK};
            }
            return new ResponseMessage { Success = false, Data = checkAlreadyExistCode, Message = "Code already exits", StatusCode = (int)HttpStatusCode.NotFound };
        }

        public ResponseMessage DeleteVoucher(int voucherId)
        {
            var checkVoucher = db.voucher.FirstOrDefault(voucher => voucher.VoucherID == voucherId);
            if (checkVoucher != null)
            {
                var checkMyVoucher = db.myVoucher.Where(voucher => voucher.VoucherID == voucherId).ToList();
                checkVoucher.QuantityUse = 0;
                db.voucher.Update(checkVoucher);
                
                foreach(var voucher in checkMyVoucher)
                {
                    voucher.IsVoucher = false;
                    db.myVoucher.Update(voucher);
                }
                db.SaveChanges();
                return new ResponseMessage {Success =true, Data = checkVoucher,Message = "Sucessfully", StatusCode= (int)HttpStatusCode.OK};
            }
            return new ResponseMessage { Success = false,Data = checkVoucher,Message ="Data not found", StatusCode=(int)HttpStatusCode.NotFound};
        }

        public ResponseMessage GetAllVouchers()
        {
            try
            {
                var vouchers = db.voucher.ToList();

                if (vouchers != null && vouchers.Any())
                {
                    return new ResponseMessage
                    {
                        Success = true,
                        Message = "Successfully",
                        Data = vouchers,
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }
                else
                {
                    return new ResponseMessage
                    {
                        Success = false,
                        Message = "Not Found",
                        Data = null,
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = "Internal Server Error",
                    Data = null,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };

            }

        }

        public ResponseMessage GetVoucherById(int voucherId)
        {
            try
            {
                var voucher = db.voucher.FirstOrDefault(c => c.VoucherID == voucherId);
                if (voucher != null)
                {
                    return new ResponseMessage
                    {
                        Success = true,
                        Message = "Successfully",
                        Data = voucher,
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }
                else
                {
                    return new ResponseMessage
                    {
                        Success = false,
                        Message = "Not Found",
                        Data = null,
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = "Internal Server Error",
                    Data = null,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }


        public ResponseMessage GetVouchersByAccountId(int accountId)
        {
            var myVoucher = db.myVoucher.Include(voucher => voucher.Voucher)
                .Where(account => account.AccountID == accountId)
                .Select(myVoucher => new
                {
                    Account = myVoucher.Account,
                    Voucher = new
                    {
                        voucherID = myVoucher.Voucher.VoucherID,
                        voucherImage = myVoucher.Voucher.VoucherImage,
                        voucherName = myVoucher.Voucher.VoucherName,
                        Code = myVoucher.Voucher.Code,
                        quantityUse = myVoucher.Voucher.QuantityUse,
                        Discount = myVoucher.Voucher.Discount,
                        description = myVoucher.Voucher.Description,
                    },
                    isVoucher = myVoucher.IsVoucher
                })
                .ToList();  
            if (myVoucher.Any())
            {
                return new ResponseMessage {Success = true, Data = myVoucher, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK};
            }

                return new ResponseMessage {Success = false, Data = myVoucher, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound};
        }

        public ResponseMessage ReceiveVoucher(int accountID, int voucherID)
        {
            try
            {
                var account = db.accounts.FirstOrDefault(account => account.AccountID == accountID);
                var voucher = db.voucher.FirstOrDefault(voucher => voucher.VoucherID == voucherID);
                if (account != null && voucher != null)
                {
                    var checkAlready = db.myVoucher.FirstOrDefault(x => x.AccountID == accountID && x.VoucherID == voucherID);
                    var voucherData = new
                    {
                        VoucherID = voucher.VoucherID,
                        VoucherName = voucher.VoucherName,
                        Code = voucher.Code,
                        QuantityUsed = voucher.QuantityUse,
                        Discount = voucher.Discount,
                        Description = voucher.Description
                    };

                    if (checkAlready != null)
                    {
                        return new ResponseMessage { Success = true, Data = voucherData, Message = "You have already received this voucher", StatusCode = (int)HttpStatusCode.AlreadyReported };

                    }
                    else
                    {
                        MyVoucher addMyVoucher = new MyVoucher
                        {
                            Account = account,
                            Voucher = voucher,
                            AccountID = account.AccountID,
                            VoucherID = voucher.VoucherID,
                            IsVoucher = true

                        };
                        db.myVoucher.Add(addMyVoucher);
                        db.SaveChanges();
                        
                        return new ResponseMessage
                        {
                            Success = true,
                            Data = voucherData,
                            Message = "Received Voucher successfully" ,
                            StatusCode = (int)HttpStatusCode.OK
                        };
                    }
                    
                }
                else
                {
                    return new ResponseMessage { Success = false, Data = null, Message = "Voucher receipt failed",StatusCode = (int)HttpStatusCode.NotFound};
                }
            }
            catch (Exception ex)
            {
                return new ResponseMessage {Success = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError};
            }
            
        }

        public ResponseMessage SearchVoucherName(string voucherName)
        {

            var searchResult = db.voucher.Where(voucher =>voucher.VoucherName.Contains(voucherName)).ToList();
            return new ResponseMessage { Success = true, Data = searchResult, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK};
        }

        public ResponseMessage UpdateVoucher(int voucherID, Voucher voucher,IFormFile image)
        {
            var getVoucher = db.voucher.FirstOrDefault(voucher => voucher.VoucherID == voucherID);
            if (getVoucher == null)
            {
                return new ResponseMessage { Success = false, Data = getVoucher, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };
            }
            else
            {
                if (image != null)
                {
                    getVoucher.VoucherImage = Ultils.Utils.SaveImage(image, environment);
                }
                getVoucher.VoucherImage = getVoucher.VoucherImage;
                getVoucher.VoucherName = voucher.VoucherName;
                getVoucher.Code = voucher.Code;
                getVoucher.QuantityUse = voucher.QuantityUse;
                getVoucher.Discount = voucher.Discount;
                getVoucher.Description = voucher.Description;
                db.voucher.Update(getVoucher);
                db.SaveChanges();
                return new ResponseMessage { Success = true,Data= getVoucher,Message = "Successfully", StatusCode= (int)HttpStatusCode.OK};
            }
        }
    }
}

