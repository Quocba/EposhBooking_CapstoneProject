using GraduationAPI_EPOSHBOOKING.DataAccess;
using GraduationAPI_EPOSHBOOKING.DTO;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using System.Threading;
using GraduationAPI_EPOSHBOOKING.Repository;

namespace GraduationAPI_EPOSHBOOKING.BackgroundService
{
    public class ClearDataBookingService : IHostedService, IDisposable
    {
        private Timer timer;
        private readonly ILogger logger;
        private readonly IServiceProvider provider;
        private readonly HashSet<string> sentEmails = new HashSet<string>();
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public ClearDataBookingService(ILogger<ClearDataBookingService> logger, IServiceProvider provider)
        {
          
            this.logger = logger;
            this.provider = provider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Clear data service running");
            timer = new Timer(Work, null, TimeSpan.FromDays(1), TimeSpan.FromDays(1));
            return Task.CompletedTask;

        }

        private void Work(object state)
        {
            if (DateTime.Now.Month == 12 && DateTime.Now.DayOfYear == 31)
            {
                logger.LogInformation($"To day is:{DateTime.Now.ToString("yyyy-MM-dd")}Runing Clear Data");
                ExportAllBookings();
                ExportBookingsForAllHotels();
                ClearData();
            }
            else
            {
                logger.LogInformation($"To day is:{DateTime.Now} Don't Runing Clear Data");
            }

        }
        public void ClearData()
        {
            try
            {
                using (var scope = provider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                    var currentDate = DateTime.Now.AddHours(14);
                    var booking = dbContext.booking
                                           .Include(x => x.Room)
                                           .Include(x => x.Room.Hotel)
                                           .Include(x => x.Account)
                                           .Include(x => x.Account.Profile)
                                           .ToList();
                    foreach (var book in booking)
                    {
                        book.Status = "Removed";
                        dbContext.booking.Update(book);
                    }
                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred.");

            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Clear Data Stopping");
            timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {

            timer?.Dispose();

        }
        public static string SendFile(string toEmail, string attachmentFilePath)
        {
            // Cấu hình thông tin SMTP
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587; // Thay đổi nếu cần
            string smtpUsername = "eposhhotel@gmail.com";
            string smtpPassword = "yqgorijrzzvpmwqa";

            try
            {
                // Tạo đối tượng SmtpClient
                using (SmtpClient client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    client.EnableSsl = true; // Sử dụng SSL để bảo vệ thông tin đăng nhập

                    // Tạo đối tượng MailMessage
                    using (MailMessage mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(smtpUsername);
                        mailMessage.To.Add(toEmail);
                        mailMessage.Subject = "[Annual Revenue Statistics Table]";
                        mailMessage.IsBodyHtml = true;
                        mailMessage.Body = "Please find the attached file.";

                        // Thêm file đính kèm
                        if (File.Exists(attachmentFilePath))
                        {
                            Attachment attachment = new Attachment(attachmentFilePath);
                            mailMessage.Attachments.Add(attachment);
                        }
                        else
                        {
                            return "Attachment file not found.";
                        }

                        // Gửi email
                        client.Send(mailMessage);
                    }
                }
                return "Email sent successfully.";
            }
            catch (Exception ex)
            {
                return $"An error occurred while sending email: {ex.Message}";
            }
        }
        public static double CheckRoomPrice(int roomID, DateTime CheckInDate, DateTime CheckOutDate)
        {
            DBContext db = new DBContext();
            var room = db.room
                         .Include(specialPrice => specialPrice.SpecialPrice)
                         .FirstOrDefault(room => room.RoomID == roomID);
            var specialPrice = room.SpecialPrice
                                   .FirstOrDefault(sp => CheckInDate >= sp.StartDate && CheckOutDate <= sp.EndDate);
            if (specialPrice != null)
            {
                room.Price = specialPrice.Price;
            }
            return room.Price;
        }
        public void ExportBookingsForAllHotels()
        {
            try
            {
                using (var scope = provider.CreateScope())
                {



                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                    var hotels = dbContext.hotel
                                          .Include(a => a.Account)
                                          .ThenInclude(p => p.Profile)
                                          .ToList();

                    foreach (var hotel in hotels)
                    {
                        var hotelID = hotel.HotelID;
                        var hotelName = hotel.Name;
                        var hotelOwnerEmail = hotel.Account.Email;
                        var hotelOwnerName = hotel.Account.Profile.fullName;

                        // Lấy danh sách các bookings cho khách sạn hiện tại
                        var bookings = dbContext.booking
                                                .Include(b => b.Room)
                                                .ThenInclude(r => r.Hotel)
                                                .Include(b => b.Account)
                                                .ThenInclude(a => a.Profile)
                                                .Where(b => b.Room.Hotel.HotelID == hotelID)
                                                .ToList();

                        if (bookings.Count == 0)
                        {
                            logger.LogInformation($"No bookings found for hotel {hotelName} (ID {hotelID}).");
                            continue;
                        }

                        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                        using (var pck = new ExcelPackage())
                        {
                            var ws = pck.Workbook.Worksheets.Add($"Booking Of {DateTime.Now.AddHours(14)}");
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
                            ws.Cells[2, 1, ws.Dimension.End.Row, headers.Length].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells.AutoFitColumns();

                            var exportFilePath = PathHelper.GetExportFileHotel();
                            bool success = false;
                            int retryCount = 5;
                            while (retryCount > 0 && !success)
                            {
                                try
                                {
                                    File.WriteAllBytes(exportFilePath, pck.GetAsByteArray());
                                    success = true;
                                }
                                catch (IOException)
                                {
                                    retryCount--;
                                    Thread.Sleep(1000);
                                }
                            }

                            if (!success)
                            {
                                logger.LogError("Unable to write the Excel file after multiple attempts.");
                            }
                            else
                            {
                                if (!sentEmails.Contains(hotelOwnerEmail))
                                {
                                    var emailResult = SendFile(hotelOwnerEmail, exportFilePath);
                                    sentEmails.Add(hotelOwnerEmail);
                                    logger.LogInformation($"Email sent for hotel {hotelOwnerEmail}: {emailResult}");
                                }
                                else
                                {
                                    logger.LogInformation($"Email already sent for hotel {hotelName}: {hotelOwnerEmail}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during data export.");
            }
        }

        public void ExportAllBookings()
        {
            try
            {
                using (var scope = provider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                    var bookings = dbContext.booking
                        .Include(b => b.Room)
                        .ThenInclude(r => r.Hotel)
                        .Include(b => b.Account)
                        .ThenInclude(a => a.Profile)
                        .ToList();

                    if (bookings.Count == 0)
                    {
                        logger.LogInformation("No bookings found.");
                        return;
                    }

                    ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                    using (var pck = new ExcelPackage())
                    {
                        var ws = pck.Workbook.Worksheets.Add("Booking List");

                        string[] headers = {
                    "Booking ID", "Check-in Date", "Check-out Date", "Total Price", "Discount Price", "Unit Price",
                    "Taxes Price", "Number of Rooms", "Number of Guests", "Cancellation Reason",
                    "Status", "Hotel Name", "Room Type", "Account Email", "Account Full Name"
                };

                        for (int i = 0; i < headers.Length; i++)
                        {
                            ws.Cells[1, i + 1].Value = headers[i];
                        }
                        ws.Cells[1, 1, 1, headers.Length].Style.Font.Bold = true;
                        ws.Cells[1, 1, 1, headers.Length].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[1, 1, 1, headers.Length].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[1, 1, 1, headers.Length].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                        int row = 2;
                        foreach (var booking in bookings)
                        {
                            int bookingDay = (booking.CheckOutDate - booking.CheckInDate).Days;
                            double roomPrice = CheckRoomPrice(booking.Room.RoomID, booking.CheckInDate, booking.CheckOutDate);
                            double totalPrice = (roomPrice * booking.NumberOfRoom) * bookingDay;
                            double totalTaxesPrice = totalPrice * 0.05;
                            double mainTotalPrice = totalPrice + totalTaxesPrice;
                            double discountPrice = booking.Voucher != null && booking.Voucher.Discount > 0
                                ? (mainTotalPrice * booking.Voucher.Discount) / 100
                                : 0;

                            ws.Cells[row, 1].Value = booking.BookingID;
                            ws.Cells[row, 2].Value = booking.CheckInDate.ToString("dd-MM-yyyy");
                            ws.Cells[row, 3].Value = booking.CheckOutDate.ToString("dd-MM-yyyy");
                            ws.Cells[row, 4].Value = mainTotalPrice + " " + "VND";
                            ws.Cells[row, 5].Value = discountPrice + " " + "VND";
                            ws.Cells[row, 6].Value = booking.UnitPrice + " " + "VND";
                            ws.Cells[row, 7].Value = booking.TaxesPrice + " " + "VND";
                            ws.Cells[row, 8].Value = booking.NumberOfRoom;
                            ws.Cells[row, 9].Value = booking.NumberGuest;
                            ws.Cells[row, 10].Value = booking.ReasonCancle;
                            ws.Cells[row, 11].Value = booking.Status;
                            ws.Cells[row, 12].Value = booking.Room?.Hotel?.Name;
                            ws.Cells[row, 13].Value = booking.Room?.TypeOfRoom;
                            ws.Cells[row, 14].Value = booking.Account?.Email;
                            ws.Cells[row, 15].Value = booking.Account?.Profile?.fullName;
                            row++;
                        }

                        ws.Cells[2, 1, ws.Dimension.End.Row, headers.Length].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells.AutoFitColumns();

                        var exportFilePath = PathHelper.GetExportFilePathForExportAllBookings();

                        try
                        {
                            File.WriteAllBytes(exportFilePath, pck.GetAsByteArray());
                            logger.LogInformation($"Excel file generated and saved to {exportFilePath}");

                            // Gửi email với file đính kèm
                            SendFile("eposhhotel@gmail.com", exportFilePath);
                            logger.LogInformation("Email sent successfully.");
                        }
                        catch (IOException ex)
                        {
                            logger.LogError(ex, "Error saving the Excel file.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during data export.");
            }
        }

        private static int exportFileCounter = 0;
        private static int clearDataFileCounter = 0;
        public static class PathHelper
        {
            // Thuộc tính tĩnh để lưu trữ đường dẫn thư mục
            public static string ExportDirectory { get; }

            static PathHelper()
            {
                
            // Xác định đường dẫn thư mục gốc
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
                ExportDirectory = Path.Combine(rootDirectory, "OldBookingData");

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(ExportDirectory))
                {
                    Directory.CreateDirectory(ExportDirectory);
                }
            }

            public static string GetExportFilePathForExportAllBookings()
            {
                exportFileCounter++;
                return Path.Combine(ExportDirectory, $"Booking System In Year {exportFileCounter}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.xlsx");
            }

            // Phương thức để lấy đường dẫn tệp cho ClearDataBookingService
            public static string GetExportFileHotel()
            {
                clearDataFileCounter++;
                return Path.Combine(ExportDirectory, $"Booking Hotel In Year {clearDataFileCounter}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.xlsx");
            }

        }
    }
}
