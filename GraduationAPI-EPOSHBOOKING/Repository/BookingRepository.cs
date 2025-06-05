using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using GraduationAPI_EPOSHBOOKING.DataAccess;
using GraduationAPI_EPOSHBOOKING.DTO;
using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Net.payOS;
using Net.payOS.Types;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SkiaSharp;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Repository
{
    public class BookingRepository : IBookingRepository
    {
        private readonly DBContext db;
        private readonly IServiceProvider serviceProvider;
        private readonly BlockingCollection<CreateBookingDTO> bookingQueue = new();
        public BookingRepository(DBContext _db, IServiceProvider serviceProvider)
        {
            this.db = _db;
            this.serviceProvider = serviceProvider;
            Task.Run(() => ProcessQueue());
        }

        public ResponseMessage CreateBooking(CreateBookingDTO newBooking)
        {
         
            bookingQueue.Add(newBooking);
        
            return HandleBooking(newBooking);
        }


        private void ProcessQueue()
        {
            foreach (var booking in bookingQueue.GetConsumingEnumerable())
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                    var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                    try
                    {
                        var response = bookingRepo.CreateBooking(booking);
                        Console.WriteLine($"Booking processed successfully: {response.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing booking: {ex.Message}");
                    }
                }
            }
        }
        public ResponseMessage HandleBooking(CreateBookingDTO newBooking)
        {
            bookingQueue.Add(newBooking);
            double unitPrice = 0;
            var account = db.accounts
                            .Include(profile => profile.Profile)
                            .FirstOrDefault(account => account.AccountID == newBooking.AccountID);
            var voucher = db.voucher.FirstOrDefault(voucher => voucher.VoucherID == newBooking.VoucherID);
            var room = db.room.Include(specialPrice => specialPrice.SpecialPrice)
                              .Include(hotel => hotel.Hotel)
                              .ThenInclude(hotel => hotel.HotelAddress)
                              .FirstOrDefault(room => room.RoomID == newBooking.RoomID);
            var specialPrice = room.SpecialPrice
                                   .FirstOrDefault(sp => newBooking.CheckInDate >= sp.StartDate && newBooking.CheckOutDate <= sp.EndDate
                                   || newBooking.CheckInDate <= sp.EndDate && newBooking.CheckInDate.Date >= sp.StartDate);
            if (specialPrice != null)
            {
                unitPrice = specialPrice.Price;
            }
            else
            {
                unitPrice = room.Price;
            }
            if (voucher != null && voucher.QuantityUse > 0)
            {
                Booking createBooking = new Booking
                {
                    Account = account,
                    Voucher = voucher,
                    Room = room,
                    CheckInDate = newBooking.CheckInDate,
                    CheckOutDate = newBooking.CheckOutDate,
                    UnitPrice = unitPrice,
                    TotalPrice = newBooking.TotalPrice,
                    TaxesPrice = newBooking.TaxesPrice,
                    NumberGuest = newBooking.NumberOfGuest,
                    NumberOfRoom = newBooking.NumberOfRoom,
                    Status = "Awaiting Check-in"
                };
                db.booking.Add(createBooking);
                voucher.QuantityUse = voucher.QuantityUse - 1;
                room.Quantity = room.Quantity - newBooking.NumberOfRoom;
                db.voucher.Update(voucher);
                db.room.Update(room);
                var myVoucher = db.myVoucher
                                  .Include(x => x.Voucher)
                                  .Include(x => x.Account)
                                  .FirstOrDefault(x => x.AccountID == account.AccountID && x.VoucherID == newBooking.VoucherID);
                if (myVoucher != null)
                {
                    myVoucher.IsVoucher = false;
                    db.myVoucher.Update(myVoucher);
                    db.SaveChanges();
                }
                if (voucher.QuantityUse == 0)
                {
                    var myVoucherList = db.myVoucher
                                           .Include(voucher => voucher.Voucher)
                                           .Include(account => account.Account)
                                           .Where(x => x.VoucherID == newBooking.VoucherID)
                                           .ToList();
                    foreach (var checkMyVoucher in myVoucherList)
                    {
                        checkMyVoucher.IsVoucher = false;
                        db.myVoucher.Update(checkMyVoucher);
                    }

                }
                Ultils.Utils.SendMailBooking(createBooking.Account.Email, createBooking);
                db.SaveChanges();

                var responseData = new
                {
                    BookingID = createBooking.BookingID,
                    CheckInDate = createBooking.CheckInDate,
                    CheckOutDate = createBooking.CheckOutDate,
                    TotalPrice = createBooking.TotalPrice,
                    UnitPrice = createBooking.UnitPrice,
                    TaxesPrice = createBooking.TaxesPrice,
                    NumberOfRoom = createBooking.NumberOfRoom,
                    NumberOfGuest = createBooking.NumberGuest,
                    Status = createBooking.Status,
                    ReasonCancel = createBooking.ReasonCancle,
                    Room = createBooking.Room == null ? null : new
                    {
                        RoomID = createBooking.Room.RoomID,
                        TypeOfRoom = createBooking.Room.TypeOfRoom,
                        NumberOfCapacity = createBooking.Room.NumberCapacity,
                        Price = createBooking.Room.Price,
                        Quantity = createBooking.Room.Quantity,
                        SizeOfRoom = createBooking.Room.SizeOfRoom,
                        TypeOfBed = createBooking.Room.TypeOfBed,
                        Hotel = createBooking.Room.Hotel == null ? null : new
                        {
                            HotelID = createBooking.Room.Hotel.HotelID,
                            MainImage = createBooking.Room.Hotel.MainImage,
                            Name = createBooking.Room.Hotel.Name,
                            OpenedIn = createBooking.Room.Hotel.OpenedIn,
                            Description = createBooking.Room.Hotel.Description,
                            HotelStandard = createBooking.Room.Hotel.HotelStandar,
                            IsRegister = createBooking.Room.Hotel.isRegister,
                            Status = createBooking.Room.Hotel.Status
                        }
                    }
                };
                return new ResponseMessage { Success = true, Data = responseData, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
            else
            {

                Booking createBooking = new Booking
                {
                    Account = account,
                    Voucher = voucher,
                    Room = room,
                    CheckInDate = newBooking.CheckInDate,
                    CheckOutDate = newBooking.CheckOutDate,
                    UnitPrice = unitPrice,
                    TotalPrice = newBooking.TotalPrice,
                    TaxesPrice = newBooking.TaxesPrice,
                    NumberGuest = newBooking.NumberOfGuest,
                    NumberOfRoom = newBooking.NumberOfRoom,
                    Status = "Awaiting Check-in"

                };
                db.booking.Add(createBooking);
                room.Quantity = room.Quantity - createBooking.NumberOfRoom;
                db.room.Update(room);
                db.booking.Add(createBooking);
                db.SaveChanges();
                Ultils.Utils.SendMailBooking(createBooking.Account.Email, createBooking);

                var myVoucher = db.myVoucher
                                  .Include(voucher => voucher.Voucher)
                                  .Include(account => account.Account)
                                  .Where(x => x.VoucherID == newBooking.VoucherID)
                                  .ToList();
                foreach (var checkVoucher in myVoucher)
                {
                    checkVoucher.IsVoucher = false;
                    db.myVoucher.Update(checkVoucher);
                }
                db.SaveChanges();
                var responseData = new
                {
                    BookingID = createBooking.BookingID,
                    CheckInDate = createBooking.CheckInDate,
                    CheckOutDate = createBooking.CheckOutDate,
                    TotalPrice = createBooking.TotalPrice,
                    UnitPrice = createBooking.UnitPrice,
                    TaxesPrice = createBooking.TaxesPrice,
                    NumberOfRoom = createBooking.NumberOfRoom,
                    NumberOfGuest = createBooking.NumberGuest,
                    Status = createBooking.Status,
                    ReasonCancel = createBooking.ReasonCancle,
                    Room = createBooking.Room == null ? null : new
                    {
                        RoomID = createBooking.Room.RoomID,
                        TypeOfRoom = createBooking.Room.TypeOfRoom,
                        NumberOfCapacity = createBooking.Room.NumberCapacity,
                        Price = createBooking.Room.Price,
                        Quantity = createBooking.Room.Quantity,
                        SizeOfRoom = createBooking.Room.SizeOfRoom,
                        TypeOfBed = createBooking.Room.TypeOfBed,
                        Hotel = createBooking.Room.Hotel == null ? null : new
                        {
                            HotelID = createBooking.Room.Hotel.HotelID,
                            MainImage = createBooking.Room.Hotel.MainImage,
                            Name = createBooking.Room.Hotel.Name,
                            OpenedIn = createBooking.Room.Hotel.OpenedIn,
                            Description = createBooking.Room.Hotel.Description,
                            HotelStandard = createBooking.Room.Hotel.HotelStandar,
                            IsRegister = createBooking.Room.Hotel.isRegister,
                            Status = createBooking.Room.Hotel.Status
                        }
                    }
                };

                return new ResponseMessage { Success = true, Data = responseData, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };

            }

        }

        public ResponseMessage GetBookingByAccount(int accountID)
        {
            var getBooking = db.booking
                               .Include(feedback => feedback.feedBacks)
                               .Include(x => x.Account)
                               .ThenInclude(x => x.Profile)
                               .Include(room => room.Room)
                               .Include(hotel => hotel.Room.Hotel)
                               .ThenInclude(address => address.HotelAddress)
                               .Where(booking => booking.Account.AccountID == accountID)
                               .ToList();
            var responseData = getBooking.Select(booking => new
            {
               BookingID = booking.BookingID,
               CheckInDate = booking.CheckInDate,
               CheckOutDate = booking.CheckOutDate,
               TotalPrice = booking.TotalPrice,
               UnitPrice = booking.UnitPrice,
               TaxesPrice = booking.TaxesPrice,
               NumberOfRoom = booking.NumberOfRoom,
               NumberOfGuest = booking.NumberGuest,
               ReasonCancle = booking.ReasonCancle,
               Status = booking.Status,
                Feedbacks = booking.feedBacks.Select(feedback => new
                {
                    FeedbackID = feedback.FeedBackID,
                    Image = feedback.Image,
                    Description = feedback.Description,
                    Rating = feedback.Rating
                }),
                Account = new
               {
                       AccountID = booking.Account.AccountID,
                       Email = booking.Account.Email,
                       Phone = booking.Account.Phone,
                       Profile = new
                       {
                           ProfileID = booking.Account.Profile.ProfileID,
                           FullName = booking.Account.Profile.fullName,
                           BirthDay = booking.Account.Profile.BirthDay,
                           Gender = booking.Account.Profile.Gender,
                           Address = booking.Account.Profile.Address,
                           Avatar = booking.Account.Profile.Avatar
                       }
               },
               Room = new
               {
                   RoomID = booking.Room.RoomID,
                   TypeOfRoom = booking.Room.TypeOfRoom,
                   NumberOfCapacity = booking.Room.NumberCapacity,
                   Price = booking.Room.Price,
                   Quantity = booking.Room.Quantity,
                   SizeOfRoom = booking.Room.SizeOfRoom,
                   TypeOfBed = booking.Room.TypeOfBed
               },
               Hotel = new
               {
                   HotelID = booking.Room.Hotel.HotelID,
                   MainImage = booking.Room.Hotel.MainImage,
                   Name = booking.Room.Hotel.Name,
                   OpenIn = booking.Room.Hotel.OpenedIn,
                   Description = booking.Room.Hotel.Description,
                   HotelStandar = booking.Room.Hotel.HotelStandar,
                   IsRegister = booking.Room.Hotel.isRegister,
                   Status = booking.Room.Hotel.Status
               },
               HotelAddress = new
               {
                   Address = booking.Room.Hotel.HotelAddress.Address,
                   City = booking.Room.Hotel.HotelAddress.City
               }
            });
            if (getBooking.Any())
            {
                return new ResponseMessage { Success = true, Data = responseData, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
            return new ResponseMessage { Success = false, Data = responseData, Message = "No Booking", StatusCode = (int)HttpStatusCode.NotFound };
        }

        public ResponseMessage CancleBooking(int bookingID, String Reason)
        {
            var booking = db.booking
                            .Include(room => room.Room)
                            .ThenInclude(hotel => hotel.Hotel)
                            .Include(account => account.Account)
                            .FirstOrDefault(booking => booking.BookingID == bookingID);
            if (booking != null)
            {
                if (CanCancelBooking(booking.CheckInDate))
                {
                    booking.Status = "Canceled";
                    booking.ReasonCancle = Reason;
                    booking.Room.Quantity = booking.Room.Quantity + booking.NumberOfRoom;
                    db.room.Update(booking.Room);
                    db.booking.Update(booking);
                    db.SaveChanges();
                    var responseData = new
                    {
                        BookingID = booking.BookingID,
                        CheckInDate = booking.CheckInDate,
                        CheckOutDate = booking.CheckOutDate,
                        TotalPrice = booking.TotalPrice,
                        UnitPrice = booking.UnitPrice,
                        TaxesPrice = booking.TaxesPrice,
                        NumberOfRoom = booking.NumberOfRoom,
                        NumberOfGuest = booking.NumberGuest,
                        Status = booking.Status,
                        ReasonCancel = booking.ReasonCancle,
                        Room = booking.Room == null ? null : new
                        {
                            RoomID = booking.Room.RoomID,
                            TypeOfRoom = booking.Room.TypeOfRoom,
                            NumberOfCapacity = booking.Room.NumberCapacity,
                            Price = booking.Room.Price,
                            Quantity = booking.Room.Quantity,
                            SizeOfRoom = booking.Room.SizeOfRoom,
                            TypeOfBed = booking.Room.TypeOfBed,
                            Hotel = booking.Room.Hotel == null ? null : new
                            {
                                HotelID = booking.Room.Hotel.HotelID,
                                MainImage = booking.Room.Hotel.MainImage,
                                Name = booking.Room.Hotel.Name,
                                OpenedIn = booking.Room.Hotel.OpenedIn,
                                Description = booking.Room.Hotel.Description,
                                HotelStandard = booking.Room.Hotel.HotelStandar,
                                IsRegister = booking.Room.Hotel.isRegister,
                                Status = booking.Room.Hotel.Status
                            }
                        }
                    };
                    return new ResponseMessage { Success = true, Data = responseData, Message = "Cancle Success", StatusCode = (int)HttpStatusCode.OK };
                };
            }

            return new ResponseMessage
            {
                Success = false,
                Message = "Cancel failed. You must cancel 24 hours before check-in date.",
                StatusCode = (int)HttpStatusCode.PaymentRequired
            };
        }

        private bool CanCancelBooking(DateTime checkInDate)
        {
            DateTime currentDateTime = DateTime.Now;
            TimeSpan timeDifference = checkInDate - currentDateTime;

            // Check if the check-in date is more than 24 hours from now
            return timeDifference.TotalHours > 24;
        }
        public ResponseMessage ChangeStatusWaitForPayment(int bookingID)
        {
            var getBooking = db.booking.FirstOrDefault(booking => booking.BookingID == bookingID);
            if (getBooking != null)
            {
                getBooking.Status = "Awaiting Payment";
                db.booking.Update(getBooking);
                db.SaveChanges();
                return new ResponseMessage { Success = true, Data = getBooking, Message = "Confirm Successfully", StatusCode= (int)HttpStatusCode.OK };
            }
            return new ResponseMessage { Success = false, Data = getBooking, Message = "Fail", StatusCode = (int)(HttpStatusCode.NotFound)};  
        }

        public ResponseMessage ChangeStatusComplete(int bookingID)
        {
            var getBooking = db.booking
                               .Include(room => room.Room)
                               .FirstOrDefault(booking => booking.BookingID==bookingID);
            if (getBooking != null)
            {
                getBooking.Status = "Completed";
                getBooking.Room.Quantity = getBooking.Room.Quantity + getBooking.NumberOfRoom;
                db.room.Update(getBooking.Room);
                db.booking.Update(getBooking);
                db.SaveChanges();
                return new ResponseMessage { Success = true, Data = getBooking, Message = "Successfully", StatusCode=(int)HttpStatusCode.OK};
            }
            return new ResponseMessage { Success = false,Data = getBooking, Message = "Fail", StatusCode =(int)(HttpStatusCode.NotFound)};
        }

        public ResponseMessage ExportBookingbyHotelID(int hotelID)
        {
            var bookings = db.booking
                 .Include(b => b.Room)
                 .ThenInclude(r => r.Hotel)
                 .Include(b => b.Account)
                 .ThenInclude(a => a.Profile)
                 .Include(voucher => voucher.Voucher)
                 .Where(b => b.Room.Hotel.HotelID == hotelID)
                 .ToList();
            if (bookings.Count == 0)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Data = null,
                    Message = "No bookings found for this hotel.",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }


            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var pck = new ExcelPackage())
            {
                var ws = pck.Workbook.Worksheets.Add("Booking List by Hotel");
                // Header row data
                string[] headers = {
            "Booking ID", "Check-in Date", "Check-out Date", "Room Price","Number Of Room", "Nights","Taxes",
            "ServiceFee", "Discount", "Total Price",
             "Status","Cancellation Reason",
            "Hotel Name", "Room Type", "Account Email", "Account Full Name"
        };
                // Add and format header row
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cells[1, i + 1].Value = headers[i];
                }
                ws.Cells[1, 1, 1, headers.Length].Style.Font.Bold = true;
                ws.Cells[1, 1, 1, headers.Length].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[1, 1, 1, headers.Length].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[1, 1, 1, headers.Length].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // Add data rows
                int row = 2;
                foreach (var booking in bookings)
                {
                    int bookingDay = (booking.CheckOutDate - booking.CheckInDate).Days;
                    double discountPrice = 0;
                    double roomPrice = booking.UnitPrice;
                    int numberOfRoom = booking.NumberOfRoom;
                    double totalPrice1 = (roomPrice * booking.NumberOfRoom) * bookingDay;
                    double Taxes = totalPrice1 * 0.05;
                    double serviceFee = totalPrice1 * 0.1;
                    double sumPrice = totalPrice1 + Taxes + serviceFee;
                    double TotalPrice = 0;
                    if (booking.Voucher != null && booking.Voucher.Discount > 0)
                    {
                        discountPrice = (sumPrice * booking.Voucher.Discount) / 100;
                    }
                    TotalPrice = sumPrice - discountPrice;


                    ws.Cells[row, 1].Value = booking.BookingID;
                    ws.Cells[row, 2].Value = booking.CheckInDate.ToString("dd-MM-yyyy");
                    ws.Cells[row, 3].Value = booking.CheckOutDate.ToString("dd-MM-yyyy");
                    ws.Cells[row, 4].Value = roomPrice + " VND"; // Room Price
                    ws.Cells[row, 5].Value = booking.NumberOfRoom; // Number Of Room
                    ws.Cells[row, 6].Value = bookingDay; // Nights
                    ws.Cells[row, 7].Value = Taxes + " VND"; // Taxes
                    ws.Cells[row, 8].Value = serviceFee + " VND"; // Service Fee
                    ws.Cells[row, 9].Value = discountPrice + " VND"; // Discount
                    ws.Cells[row, 10].Value = TotalPrice + " VND"; // Total Price
                    ws.Cells[row, 11].Value = booking.Status; // Status
                    ws.Cells[row, 12].Value = booking.ReasonCancle; // Cancellation Reason
                    ws.Cells[row, 13].Value = booking.Room?.Hotel?.Name; // Hotel Name
                    ws.Cells[row, 14].Value = booking.Room?.TypeOfRoom; // Room Type
                    ws.Cells[row, 15].Value = booking.Account?.Email; // Account Email
                    ws.Cells[row, 16].Value = booking.Account?.Profile?.fullName; // Account Full Name
                    row++;
                }
                // Định dạng dữ liệu
                ws.Cells[2, 1, ws.Dimension.End.Row, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells.AutoFitColumns();
                var listBookingWithMonth = db.booking
                    .Where(booking => booking.Room.Hotel.HotelID == hotelID)
                    .GroupBy(booking => new
                    {
                        CheckInMonth = booking.CheckInDate.Month,
                        CheckOutMonth = booking.CheckOutDate.Month,
                        CheckInYear = booking.CheckInDate.Year
                    }).ToList();
                var totalWithMonth = new Dictionary<string, double>();
                foreach (var months in listBookingWithMonth)
                {
                    var checkInMonth = months.Key.CheckInMonth;
                    var checkInYear = months.Key.CheckInYear;
                    var totalRevenueForMonth = 0.0;
                    foreach (var booking in months)
                    {
                        totalRevenueForMonth += booking.TotalPrice;
                    }
                    var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(checkInMonth);
                    var monthYear = $"{monthName}/{checkInYear}";
                    if (!totalWithMonth.ContainsKey(monthYear))
                    {
                        totalWithMonth.Add(monthYear, totalRevenueForMonth);
                    }
                    else
                    {
                        totalWithMonth[monthYear] += totalRevenueForMonth;
                    }
                }
                var result = totalWithMonth.Select(booking => new BookingRevenuesData
                {
                    Name = booking.Key,
                    Data = booking.Value
                });
                var ws2 = pck.Workbook.Worksheets.Add("Booking Revenues by Hotel");
                ws2.Cells[1, 1].Value = "Month";
                ws2.Cells[1, 2].Value = "Total Revenue";
                int row2 = 2;
                foreach (var booking in result)
                {
                    ws2.Cells[row2, 1].Value = booking.Name;
                    ws2.Cells[row2, 2].Value = booking.Data + " " + "VND";
                    row2++;
                }
                ws2.Cells[1, 1, 1, 2].Style.Font.Bold = true;
                ws2.Cells[1, 1, 1, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws2.Cells[1, 1, 1, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                ws2.Cells[1, 1, 1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws2.Cells[2, 1, ws2.Dimension.End.Row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws2.Cells.AutoFitColumns();
                // Save to memory stream    
                MemoryStream stream = new MemoryStream();
                pck.SaveAs(stream);
                return new ResponseMessage
                {
                    Success = true,
                    Data = stream.ToArray(),
                    Message = "Excel file generated successfully.",
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
        }
        public ResponseMessage GetAllBooking()
        {
            try
            {
                var bookings = db.booking
                                  .Include(b => b.Room)
                                  .ThenInclude(hotel => hotel.Hotel)
                                  .Include(b => b.Account)
                                  .ThenInclude(profile => profile.Profile)
                                  .ToList();

                return new ResponseMessage { Success = true, Data = bookings, Message = "Successfully retrieved all bookings.", StatusCode = (int)HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                return new ResponseMessage { Success = false, Data = null, Message = "Internal Server Error", StatusCode = (int)HttpStatusCode.InternalServerError };
            }
        } // chưa biết có dùng hay không

        public ResponseMessage ExportBookingsByAccountID(int accountID)
        {
            try
            {
                var bookings = db.booking
                    .Include(booking => booking.Room)
                    .ThenInclude(room => room.Hotel)
                    .Include(booking => booking.Voucher)
                    .Include(booking => booking.Account)
                    .Where(booking => booking.Account.AccountID == accountID)
                    .ToList();

                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Booking List");
                    // Define headers
                    string[] headers = {
                "BookingID", "CheckInDate", "CheckOutDate", "RoomPrice", "NumberOfRoom", "Nights", "Taxes", "Service Fee",
                "Discount", "Total Price", "Status", "ReasonCancel"
            };


                    // Set column headers
                    for (int i = 0; i < headers.Length; i++)
                    {
                        ws.Cells[1, i + 1].Value = headers[i];
                    }

                    // Format header row
                    ws.Cells[1, 1, 1, headers.Length].Style.Font.Bold = true;
                    ws.Cells[1, 1, 1, headers.Length].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[1, 1, 1, headers.Length].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    ws.Cells[1, 1, 1, headers.Length].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Populate data rows
                    int row = 2;
                    foreach (var booking in bookings)
                    {
                        int bookingDay = (booking.CheckOutDate - booking.CheckInDate).Days;
                        double discountPrice = 0;
                        double roomPrice = booking.UnitPrice;
                        int numberOfRoom = booking.NumberOfRoom;
                        double totalPrice1 = (roomPrice * booking.NumberOfRoom) * bookingDay;
                        double Taxes = totalPrice1 * 0.05;
                        double serviceFee = totalPrice1 * 0.1;
                        double sumPrice = totalPrice1 + Taxes + serviceFee;
                        double TotalPrice = sumPrice;

                        if (booking.Voucher != null && booking.Voucher.Discount > 0)
                        {
                            discountPrice = (TotalPrice * booking.Voucher.Discount) / 100;
                            TotalPrice = sumPrice - discountPrice;
                        }

                        ws.Cells[row, 1].Value = booking.BookingID;
                        ws.Cells[row, 2].Value = booking.CheckInDate.ToString("dd-MM-yyyy");
                        ws.Cells[row, 3].Value = booking.CheckOutDate.ToString("dd-MM-yyyy");
                        ws.Cells[row, 4].Value = roomPrice + " VND";
                        ws.Cells[row, 5].Value = booking.NumberOfRoom;
                        ws.Cells[row, 6].Value = bookingDay;  // Số đêm
                        ws.Cells[row, 7].Value = Taxes + " VND";
                        ws.Cells[row, 8].Value = serviceFee + " VND";
                        ws.Cells[row, 9].Value = discountPrice + " VND";
                        ws.Cells[row, 10].Value = TotalPrice + " VND";
                        ws.Cells[row, 11].Value = booking.Status;
                        ws.Cells[row, 12].Value = booking.ReasonCancle ?? "N/A"; // Kiểm tra null cho ReasonCancel
                        row++;
                    }


                    // Format data rows
                    ws.Cells[2, 1, ws.Dimension.End.Row, headers.Length].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells.AutoFitColumns();

                    MemoryStream stream = new MemoryStream();
                    pck.SaveAs(stream);

                    return new ResponseMessage
                    {
                        Success = true,
                        Data = stream.ToArray(),
                        Message = bookings.Any() ? "Excel file generated successfully." : "No bookings found. Empty file generated.",
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Data = null,
                    Message = "Error generating Excel file.",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }
        public ResponseMessage ExportAllBookings()
        {
            try
            {
                var bookings = db.booking
                    .Include(b => b.Room)
                    .ThenInclude(r => r.Hotel)
                    .Include(b => b.Account)
                    .ThenInclude(a => a.Profile)
                    .ToList();

                if (bookings.Count == 0)
                {
                    return new ResponseMessage
                    {
                        Success = false,
                        Data = null,
                        Message = "No bookings found.",
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }

                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                using (var pck = new ExcelPackage())
                {
                    var ws = pck.Workbook.Worksheets.Add("Booking List");

                    // Header row data
                    string[] headers = {
                    "Booking ID", "Check-in Date", "Check-out Date", "Total Price", "Unit Price",
                    "Taxes Price", "Number of Rooms", "Number of Guests", "Cancellation Reason",
                    "Status", "Hotel Name", "Room Type", "Account Email", "Account Full Name"
                    };

                    // Add and format header row
                    for (int i = 0; i < headers.Length; i++)
                    {
                        ws.Cells[1, i + 1].Value = headers[i];
                    }
                    ws.Cells[1, 1, 1, headers.Length].Style.Font.Bold = true;
                    ws.Cells[1, 1, 1, headers.Length].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[1, 1, 1, headers.Length].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[1, 1, 1, headers.Length].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                    // Add data rows
                    int row = 2;
                    foreach (var booking in bookings)
                    {
                        ws.Cells[row, 1].Value = booking.BookingID;
                        ws.Cells[row, 2].Value = booking.CheckInDate.ToString("dd-MM-yyyy");
                        ws.Cells[row, 3].Value = booking.CheckOutDate.ToString("dd-MM-yyyy");
                        ws.Cells[row, 4].Value = booking.TotalPrice + " " + "VND";
                        ws.Cells[row, 5].Value = booking.UnitPrice + " " + "VND";
                        ws.Cells[row, 6].Value = booking.TaxesPrice + " " + "VND";
                        ws.Cells[row, 7].Value = booking.NumberOfRoom;
                        ws.Cells[row, 8].Value = booking.NumberGuest;
                        ws.Cells[row, 9].Value = booking.ReasonCancle;
                        ws.Cells[row, 10].Value = booking.Status;
                        ws.Cells[row, 11].Value = booking.Room?.Hotel?.Name;
                        ws.Cells[row, 12].Value = booking.Room?.TypeOfRoom;
                        ws.Cells[row, 13].Value = booking.Account?.Email;
                        ws.Cells[row, 14].Value = booking.Account?.Profile?.fullName;
                        row++;
                    }
                    // Định dạng dữ liệu
                    ws.Cells[2, 1, ws.Dimension.End.Row, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells.AutoFitColumns();
                    var listBookingWithMonth = db.booking
                        .GroupBy(booking => new
                        {
                            CheckInMonth = booking.CheckInDate.Month,
                            CheckOutMonth = booking.CheckOutDate.Month,
                            CheckInYear = booking.CheckInDate.Year
                        }).ToList();
                    var totalWithMonth = new Dictionary<string, double>();
                    foreach (var months in listBookingWithMonth)
                    {
                        var checkInMonth = months.Key.CheckInMonth;
                        var checkInYear = months.Key.CheckInYear;
                        var totalRevenueForMonth = 0.0;
                        foreach (var booking in months)
                        {
                            totalRevenueForMonth += booking.TotalPrice;
                        }
                        var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(checkInMonth);
                        var monthYear = $"{monthName}/{checkInYear}";
                        if (!totalWithMonth.ContainsKey(monthYear))
                        {
                            totalWithMonth.Add(monthYear, totalRevenueForMonth);
                        }
                        else
                        {
                            totalWithMonth[monthYear] += totalRevenueForMonth;
                        }
                    }
                    var result = totalWithMonth.Select(booking => new BookingRevenuesData
                    {
                        Name = booking.Key,
                        Data = booking.Value
                    });
                    var ws2 = pck.Workbook.Worksheets.Add("Booking Revenues");
                    ws2.Cells[1, 1].Value = "Month";
                    ws2.Cells[1, 2].Value = "Total Revenue";
                    int row2 = 2;
                    foreach (var booking in result)
                    {
                        ws2.Cells[row2, 1].Value = booking.Name;
                        ws2.Cells[row2, 2].Value = booking.Data + " " + "VND";
                        row2++;
                    }
                    ws2.Cells[1, 1, 1, 2].Style.Font.Bold = true;
                    ws2.Cells[1, 1, 1, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws2.Cells[1, 1, 1, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    ws2.Cells[1, 1, 1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws2.Cells[2, 1, ws2.Dimension.End.Row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws2.Cells.AutoFitColumns();
                    // Save to memory stream
                    MemoryStream stream = new MemoryStream();
                    pck.SaveAs(stream);

                    return new ResponseMessage
                    {
                        Success = true,
                        Data = stream.ToArray(),
                        Message = "Excel file generated successfully.",
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }
            }

            catch (Exception ex)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Data = null,
                    Message = "Error generating Excel file.",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }

        public ResponseMessage ExportRevenues()
        {
            try
            {
                var bookings = db.booking
                    .Include(b => b.Room)
                    .ThenInclude(r => r.Hotel)
                    .Include(b => b.Account)
                    .ThenInclude(a => a.Profile)
                    .ToList();

                if (bookings.Count == 0)
                {
                    return new ResponseMessage
                    {
                        Success = false,
                        Data = null,
                        Message = "No bookings found.",
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }

                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                using (var pck = new ExcelPackage())
                {

                    var listBookingWithMonth = db.booking
                        .Where(x => x.Status.Equals("Completed"))
                        .GroupBy(booking => new
                        {
                            CheckInMonth = booking.CheckInDate.Month,
                            CheckOutMonth = booking.CheckOutDate.Month,
                            CheckInYear = booking.CheckInDate.Year
                        }).ToList();
                    var totalWithMonth = new Dictionary<string, double>();
                    foreach (var months in listBookingWithMonth)
                    {
                        var checkInMonth = months.Key.CheckInMonth;
                        var checkInYear = months.Key.CheckInYear;
                        var totalRevenueForMonth = 0.0;
                        foreach (var booking in months)
                        {
                            totalRevenueForMonth += booking.TotalPrice;
                        }
                        var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(checkInMonth);
                        var monthYear = $"{monthName}/{checkInYear}";
                        if (!totalWithMonth.ContainsKey(monthYear))
                        {
                            totalWithMonth.Add(monthYear, totalRevenueForMonth);
                        }
                        else
                        {
                            totalWithMonth[monthYear] += totalRevenueForMonth;
                        }
                    }
                    var result = totalWithMonth.Select(booking => new BookingRevenuesData
                    {
                        Name = booking.Key,
                        Data = booking.Value
                    });
                    var ws2 = pck.Workbook.Worksheets.Add("Booking Revenues");
                    ws2.Cells[1, 1].Value = "Month";
                    ws2.Cells[1, 2].Value = "Total Revenue";
                    int row2 = 2;
                    foreach (var booking in result)
                    {
                        ws2.Cells[row2, 1].Value = booking.Name;
                        ws2.Cells[row2, 2].Value = booking.Data + " " + "VND";
                        row2++;
                    }
                    ws2.Cells[1, 1, 1, 2].Style.Font.Bold = true;
                    ws2.Cells[1, 1, 1, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws2.Cells[1, 1, 1, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    ws2.Cells[1, 1, 1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws2.Cells[2, 1, ws2.Dimension.End.Row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws2.Cells.AutoFitColumns();
                    // Save to memory stream
                    MemoryStream stream = new MemoryStream();
                    pck.SaveAs(stream);

                    return new ResponseMessage
                    {
                        Success = true,
                        Data = stream.ToArray(),
                        Message = "Excel file generated successfully.",
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }
            }

            catch (Exception ex)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Data = null,
                    Message = "Error generating Excel file.",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }

        public ResponseMessage AnalysisRevenueBookingSystem()
        {
            var listBookingWithMonth = db.booking
                                         .Where(x => x.Status.Equals("Completed"))
                                         .GroupBy(booking => new
                                         {
                                             CheckInDate = booking.CheckInDate.Month,
                                             CheckOutDate = booking.CheckOutDate.Month,
                                         }).ToList();
            var totalWithMonth = new Dictionary<int, double>();
            foreach (var months in listBookingWithMonth)
            {
                var checkInMonth = months.Key.CheckInDate;
                var totalRevenueForMonth = 0.0;
                foreach (var booking in months)
                {
                    totalRevenueForMonth += booking.TotalPrice;
                }
                if (!totalWithMonth.ContainsKey(checkInMonth))
                {
                    totalWithMonth.Add(checkInMonth, totalRevenueForMonth);
                }
                else
                {
                    totalWithMonth[checkInMonth] += totalRevenueForMonth;
                }
            }
            var monthName = new[]
            {
                "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
                 };
            var result = totalWithMonth.Select(booking => new BookingRevenuesData
            {
                Name = monthName[booking.Key - 1],
                Data = booking.Value
            });
            return new ResponseMessage { Success = true,Data = result,Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
        }

        public ResponseMessage AnalysisRevenueBookingHotel(int hotelID)
        {
            var listHotelBookingWithMonth = db.booking
                                                 .Include(room => room.Room)
                                                 .ThenInclude(hotel => hotel.Hotel)
                                                 .Where(booking => booking.Room.Hotel.HotelID == hotelID && booking.Status.Equals("Completed"))
                                                 .ToList();

            var totalWithMonth = new Dictionary<int, (double TotalRevenue, int BookingCount)>();

            // Khởi tạo tất cả các tháng với giá trị 0
            for (int i = 1; i <= 12; i++)
            {
                totalWithMonth[i] = (0.0, 0);
            }

            foreach (var booking in listHotelBookingWithMonth)
            {
                var checkInMonth = booking.CheckInDate.Month;
                var currentData = totalWithMonth[checkInMonth];
                totalWithMonth[checkInMonth] = (currentData.TotalRevenue + booking.TotalPrice, currentData.BookingCount + 1);
            }

            var monthName = new[]
            {
                "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    };

            var result = totalWithMonth.Select(booking => new
            {
                Month = monthName[booking.Key - 1],
                Booking = booking.Value.BookingCount,
                Revenue = booking.Value.TotalRevenue
            }).ToList();

            return new ResponseMessage { Success = true, Data = result, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
        }

        public ResponseMessage CountBookingSystem()
        {
            var listBookingWithMonth = db.booking
                                         .Where(booking => booking.Status.Equals("Completed"))
                                         .GroupBy(booking => new
                                        {
                                           CheckInDate = booking.CheckInDate,
                                           CheckOutDate = booking.CheckOutDate
                                         }).ToList();
            var QuantityBookingWithMonth = new Dictionary<int, int>();

            foreach (var months in listBookingWithMonth)
            {
                var CheckInMonth = months.Key.CheckInDate.Month;

                if (!QuantityBookingWithMonth.ContainsKey(CheckInMonth))
                {
                    QuantityBookingWithMonth.Add(CheckInMonth, months.Count());
                }
                else
                {
                    QuantityBookingWithMonth[CheckInMonth] += months.Count();
                }
            }
            var monthName = new[]
            {
                "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
            };
            var result = QuantityBookingWithMonth.Select(qb => new BookingDataDTO
            {
                name = monthName[qb.Key - 1],
                Data = qb.Value
            }).ToList();
            return new ResponseMessage { Success = true, Data = result, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };

        }

        public ResponseMessage CountBookingHotel(int hotelID)
        {
            var listBookingWithMonth = db.booking
                                         .Include(room => room.Room)
                                         .ThenInclude(hotel => hotel.Hotel)
                                         .Where(booking => booking.Room.Hotel.HotelID == hotelID && booking.Status.Equals("Completed"))
                                        .GroupBy(booking => new
                                        {
                                            CheckInDate = booking.CheckInDate,
                                            CheckOutDate = booking.CheckOutDate
                                        }).ToList();
            var QuantityBookingWithMonth = new Dictionary<int, int>();

            foreach (var months in listBookingWithMonth)
            {
                var CheckInMonth = months.Key.CheckInDate.Month;

                if (!QuantityBookingWithMonth.ContainsKey(CheckInMonth))
                {
                    QuantityBookingWithMonth.Add(CheckInMonth, months.Count());
                }
                else
                {
                    QuantityBookingWithMonth[CheckInMonth] += months.Count();
                }
            }

            var monthName = new[]
            {
                "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
            };
            var result = QuantityBookingWithMonth.Select(qb => new BookingDataDTO
            {
                name = monthName[qb.Key - 1],
                Data = qb.Value
            }).ToList();
            return new ResponseMessage { Success = true, Data = result, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };

        }

        public ResponseMessage Top5Booking()
        {
            var listBooking = db.booking
                                .Include(account => account.Account)
                                .ThenInclude(profile => profile.Profile)
                                .Include(room => room.Room)
                                .ThenInclude(hotel => hotel.Hotel)
                                .Where(booking => booking.Status.Equals("Completed"))
                                .ToList();
            var top5Booking = listBooking.GroupBy(account => account.Account.AccountID)
                                         .Select(group => new
                                         {
                                             AccountID = group.Key,
                                             TotalBooking = group.Count(),
                                             Spending = group.Sum(booking => booking.TotalPrice),
                                             Account = group.First().Account
                                         })
                                         .OrderByDescending(spending => spending.Spending)
                                         .Select(top => new Top5BookingDTO
                                         {
                                             avatar = top.Account.Profile.Avatar,
                                             fullName = top.Account.Profile.fullName,
                                             TotalBooking = top.TotalBooking,
                                             Spending = top.Spending
                                         }).ToList(); 
            return new ResponseMessage { Success = true,Data = top5Booking, Message = "Top 5 Booking", StatusCode= (int)HttpStatusCode.OK };
        }

        public ResponseMessage GetBookingByHotel(int hotelID)
        {
            var listBooking = db.booking
                                .Include(account => account.Account)
                                .ThenInclude(profile => profile.Profile)
                                .Include(room => room.Room)
                                .ThenInclude(roomImage => roomImage.RoomImages)
                                .Include(service => service.Room.RoomService)
                                .ThenInclude(subService => subService.RoomSubServices)
                                .Include(hotel => hotel.Room.Hotel)
                                .Include(voucher => voucher.Voucher)
                                .Where(h => h.Room.Hotel.HotelID == hotelID)
                                .ToList().Select(booking => new
                                {
                                    BookingID = booking.BookingID,
                                    CheckInDate = booking.CheckInDate,
                                    CheckOutDate = booking.CheckOutDate,
                                    UnitPrice = booking.UnitPrice,
                                    TotalPrice = booking.TotalPrice,
                                    TaxesPrice = booking.TaxesPrice,
                                    NubmerOfRoom = booking.NumberOfRoom,
                                    NumberOfGuest = booking.NumberGuest,
                                    ReasonCancle = booking.ReasonCancle,
                                    Status = booking.Status,
                                    Voucher = booking.Voucher != null ? new
                                    {
                                        VoucherID = booking.Voucher.VoucherID,
                                        VoucherName = booking.Voucher.VoucherName,
                                        Code = booking.Voucher.Code,
                                        QuantityUse = booking.Voucher.QuantityUse,
                                        Discount = booking.Voucher.Discount,
                                        Description = booking.Voucher.Description
                                    } : null,
                                    Room = new
                                    {
                                       RoomID = booking.Room.RoomID,
                                       TypeOfRoom = booking.Room.TypeOfRoom,
                                       NumberCapacity = booking.Room.NumberCapacity,
                                       Price = booking.Room.Price,
                                       Quantity = booking.Room.Quantity,
                                       SizeOfRoom = booking.Room.SizeOfRoom,
                                       TypeOfBed = booking.Room.TypeOfBed,
                                       NumberBed = booking.Room.NumberOfBed,
                                       RoomImage = booking.Room.RoomImages.Select(img => new
                                       {
                                           Image = img.Image
                                       }).ToList(),
                                        RoomService = booking.Room.RoomService.Select(service => new
                                        {
                                            ServiceName = service.Type,
                                            RoomSubServices = service.RoomSubServices.Select(subService => new
                                            {
                                                SubServiceName = subService.SubName
                                            }).ToList()
                                        }).ToList()
                                    },
                                   
                                    Account = new
                                    {
                                        Email  = booking.Account.Email,
                                        Phone = booking.Account.Phone,

                                    },
                                    Profile = new
                                    {
                                        FullName = booking.Account.Profile.fullName,
                                        BirthDay = booking.Account.Profile.BirthDay,
                                        Gender = booking.Account.Profile.Gender,
                                        Address = booking.Account.Profile.Address,
                                        Avatar = booking.Account.Profile.Avatar
                                    }
                                });
            return new ResponseMessage { Success = true,Data = listBooking,Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
                                
        }

        public double CheckRoomPrice(int roomID, DateTime CheckInDate, DateTime CheckOutDate)
        {
            int bookingDays = (CheckOutDate - CheckInDate).Days;
            var room = db.room
                         .Include(specialPrice => specialPrice.SpecialPrice)
                         .FirstOrDefault(room => room.RoomID == roomID);
            var specialPrice = room.SpecialPrice
                                   .FirstOrDefault(sp => CheckInDate >= sp.StartDate && CheckOutDate <= sp.EndDate
                                   || CheckInDate <= sp.EndDate && CheckInDate >= sp.StartDate);
      
            if (specialPrice != null)
            {
                room.Price = specialPrice.Price;
            }
            return room.Price;  
        }

        public async Task<string> GeneratePaymentLink(PaymentRequestDTO request)    
        {
             string clientId = "8e20b398-7a60-4ea7-ae11-c8b1267de435";
             string apiKey = "aa5333b5-b17c-4428-89de-2613fa7847ec";
             string checksumKey = "d00595dd40ad4898adf0cf4d636a26c237d8f0a72807074c111d67a0a426855e";

            var payOs = new PayOS(clientId,apiKey,checksumKey);
        var items = new List<ItemData>
             {
                 new ItemData(request.Description, request.TotalPrice, request.TotalPrice)
              };

            var paymentData = new PaymentData(
                request.BookingID,
                request.TotalPrice,
                request.Description,
                items,
                request.SuccessUrl,
                request.FailureUrl
            );

            var createPayment = await payOs.createPaymentLink(paymentData);
            return createPayment.checkoutUrl;
        }

        public ResponseMessage GetBookingDetails(int bookingID)
        {
            var booking = db.booking
                            .Include(feedback => feedback.feedBacks)
                            .Include(voucher => voucher.Voucher)
                            .Include(BookingAccount => BookingAccount.Account)
                            .ThenInclude(profile => profile.Profile)
                            .Include(r => r.Room)
                            .Include(h => h.Room.Hotel)
                            .Include(adress => adress.Room.Hotel.HotelAddress)
                            .Include(partner => partner.Room.Hotel.Account)
                            .Include(partnerProfile => partnerProfile.Room.Hotel.Account.Profile)
                            .FirstOrDefault(x => x.BookingID == bookingID);
            if (booking != null)
            {
                var responseData = new
                {
                    BookingID = booking.BookingID,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    TotalPrice = booking.TotalPrice,
                    UnitPrice = booking.UnitPrice,
                    TaxesPrice = booking.TaxesPrice,
                    NumberOfRoom = booking.NumberOfRoom,
                    NumberOfGuest = booking.NumberGuest,
                    ResaonCacle = booking.ReasonCancle,
                    Status = booking.Status,
                    Feedbacks = booking.feedBacks.Select(feedback => new
                    {
                        FeedbackID = feedback.FeedBackID,
                        Image = feedback.Image,
                        Description = feedback.Description,
                        Rating = feedback.Rating,
                      
                    }),
                    Voucher = booking.Voucher != null ? new
                    {
                        VoucherID = booking.Voucher.VoucherID,
                        VoucherImage = booking.Voucher.VoucherImage,
                        Code = booking.Voucher.Code,
                        QuantityUse = booking.Voucher.QuantityUse,
                        Discount = booking.Voucher.Discount,
                        Description = booking.Voucher.Description
                    } : null,
                    BookingAccount = new
                    {
                        AccountID = booking.Account.AccountID,
                        Email = booking.Account.Email,
                        Phone = booking.Account.Phone,
                        Profile = new
                        {
                            FullName = booking.Account.Profile.fullName,
                            BirthDay = booking.Account.Profile.BirthDay,
                            Gender = booking.Account.Profile.Gender,
                            Address = booking.Account.Profile.Address,
                            Avatar = booking.Account.Profile.Avatar
                        }
                    },
                    Room = new
                    {
                        RoomID = booking.Room.RoomID,
                        TypeOfRoom = booking.Room.TypeOfRoom,
                        NumberCapacity = booking.Room.NumberCapacity,
                        Price = booking.Room.Price,
                        Quantity = booking.Room.Quantity,
                        SizeOfRoom = booking.Room.SizeOfRoom,
                        TypeOfBed = booking.Room.TypeOfBed,
                        NumberOfBed = booking.Room.NumberOfBed,
                        Hotel = new
                        {
                            HotelID = booking.Room.Hotel.HotelID,
                            MainImage = booking.Room.Hotel.MainImage,
                            Name  = booking.Room.Hotel.Name,
                            OnpenedIn = booking.Room.Hotel.OpenedIn,
                            Description = booking.Room.Hotel.Description,
                            HotelStandar = booking.Room.Hotel.HotelStandar,
                            HotelAddress = new
                            {
                                AddressID = booking.Room.Hotel.HotelAddress.AddressID,
                                Address = booking.Room.Hotel.HotelAddress.Address

                            },
                            ParnerAccount = new
                            {
                                Email = booking.Room.Hotel.Account.Email,
                                Phone = booking.Room.Hotel.Account.Phone,
                                PartnerProfile = new
                                {
                                    FullName = booking.Room.Hotel.Account.Profile.fullName,
                                    BirthDay = booking.Room.Hotel.Account.Profile.BirthDay,
                                    Gender = booking.Room.Hotel.Account.Profile.Gender,
                                    Address = booking.Room.Hotel.Account.Profile.Address,
                                    Avatar = booking.Room.Hotel.Account.Profile.Avatar
                                }
                            }
                        }
                    }
                };
                        
                return new ResponseMessage { Success = true, Message = "Successfully",Data = responseData, StatusCode = (int)HttpStatusCode.OK };
            }

            return new ResponseMessage { Success = false, Message = "Data not found", Data = booking, StatusCode = (int)HttpStatusCode.NotFound };

        }
    }
}
