using GraduationAPI_EPOSHBOOKING.Model;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Drawing;
using ClosedXML.Excel;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.Extensions.Configuration;
using GraduationAPI_EPOSHBOOKING.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.Upload;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Account = GraduationAPI_EPOSHBOOKING.Model.Account;

namespace GraduationAPI_EPOSHBOOKING.Ultils
{
    public class Utils
    {
        public static class AppSettings
        {
            public static string Token { get; } = "my top secret key";
        }
        private static readonly string[] VietnameseSigns = new string[]
{
        "aAeEoOuUiIdDyY",
        "áàạảãâấầậẩẫăắằặẳẵ",
        "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
        "éèẹẻẽêếềệểễ",
        "ÉÈẸẺẼÊẾỀỆỂỄ",
        "óòọỏõôốồộổỗơớờợởỡ",
        "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
        "úùụủũưứừựửữ",
        "ÚÙỤỦŨƯỨỪỰỬỮ",
        "íìịỉĩ",
        "ÍÌỊỈĨ",
        "đ",
        "Đ",
        "ýỳỵỷỹ",
        "ÝỲỴỶỸ"
};
        public static string RemoveVietnameseDiacritics(string text)
        {
            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)
                {
                    text = text.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
                }
            }
            return text;
        }

        public static IConfiguration configuration;

        public static string CreateToken(Account? accounts, IConfiguration configuration)
        {
            if (accounts == null)
            {
                throw new ArgumentException("Account is null or empty.");
            }

            var tokenValue = configuration.GetSection("AppSettings:Token").Value;
            if (string.IsNullOrEmpty(tokenValue))
            {
                throw new InvalidOperationException("Token key is missing or invalid in configuration.");
            }

            List<Claim> claims = new List<Claim>();

            if (accounts.Role == null || string.IsNullOrWhiteSpace(accounts.Role.Name))
            {
                throw new ArgumentException("Account role is null or empty.");
            }

            claims.Add(new Claim(ClaimTypes.Role, accounts.Role.Name.ToLower()));
            claims.Add(new Claim("AccountID", accounts.AccountID.ToString()));
            
            //claims.Add(new Claim(ClaimTypes.Email, accounts.Email));
            //claims.Add(new Claim(ClaimTypes.MobilePhone, accounts.Phone));
            if (accounts.Email != null)
            {
                claims.Add(new Claim(ClaimTypes.Email, accounts.Email));
            }

            if (accounts.Phone != null)
            {
                claims.Add(new Claim(ClaimTypes.MobilePhone, accounts.Phone));
            }

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(tokenValue));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: cred);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        public static Account GetUserInfoFromToken(string token, IConfiguration configuration)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:Token").Value);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out _);
                var roleClaim = claimsPrincipal.FindFirst(ClaimTypes.Role);
                var emailClaim = claimsPrincipal.FindFirst(ClaimTypes.Email);
                var phoneClaim = claimsPrincipal.FindFirst(ClaimTypes.MobilePhone);
                var accountIdClaim = claimsPrincipal.FindFirst("AccountID");

                if (roleClaim == null)
                {
                    return null;
                }

                return new Account
                {
                    Role = new Model.Role { Name = roleClaim.Value },
                    Email = emailClaim?.Value,  // Email có thể null
                    Phone = phoneClaim?.Value, // Phone có thể null
                    AccountID = int.Parse(accountIdClaim?.Value)
                };
            
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static string HashPassword(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }


        public static string ConvertToBase64(IFormFile image)
        {
            using (var ms = new MemoryStream())
            {
                image.CopyTo(ms);
                var fileBytes = ms.ToArray();
                return Convert.ToBase64String(fileBytes);
            }
        }

        public static string SaveImage(IFormFile image, IWebHostEnvironment environment)
        {
            if (image == null || environment == null)
            {
                throw new ArgumentNullException("Invalid image or environment settings.");
            }

            string uploadsFolder = Path.Combine(environment.ContentRootPath, "images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            string uniqueFileName = "";
            if (image.FileName == null)
            {
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.Name);
            }
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(fileStream);
            }

            return "/images/" + uniqueFileName;
        }

        public static String sendMail(String toEmail)
        {
            Random random = new Random();
            int otpNumber = random.Next(100000, 999999);
            // Tạo mã OTP ngẫu nhiên
            string otp = otpNumber.ToString();

            // Cấu hình thông tin SMTP
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587; // Thay đổi nếu cần
            string smtpUsername = "eposhhotel@gmail.com";
            string smtpPassword = "yqgorijrzzvpmwqa";
            //String smtpUsername = "hoteleposh@gmail.com";
            //String smtpPassword = "tfymnzpuljryztjn";

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
                    mailMessage.Subject = "EPOSH-BOOKING OTP CODE";
                    mailMessage.Body = $"Your OTP code is: {otp}";

                    // Gửi email
                    client.Send(mailMessage);
                }

            }
            return otp;
        }
        public static byte[] ConvertIFormFileToByteArray(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            return null;
        }

        public static string ConvertIFormFileToBase64(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);
                    byte[] fileBytes = memoryStream.ToArray();
                    return Convert.ToBase64String(fileBytes);
                }
            }
            return null;
        }
        public static string GenerateRandomString()
        {
            int length = 5;
            const string prefix = "EPOSH";
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            char[] stringChars = new char[length - prefix.Length]; // Đặt độ dài chuỗi ngẫu nhiên để giảm đi độ dài của prefix

            for (int i = 0; i < length - prefix.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }


            return prefix + new string(stringChars);
        }

       public static String GenerateStringVoucher(int length = 6)
        {
            Random random = new Random();
            const string randomString = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                int randomIndex = random.Next(randomString.Length);
                stringBuilder.Append(randomString[randomIndex]);
            }
            return stringBuilder.ToString();
        }


        public static String SendMailRegistration([FromHeader] string toEmail, String content)
        {
            try
            {
                // Cấu hình thông tin SMTP
                string smtpServer = "smtp.gmail.com";
                int smtpPort = 587; // Thay đổi nếu cần
                //string smtpUsername = "eposhhotel@gmail.com";
                //string smtpPassword = "yqgorijrzzvpmwqa";
                String smtpUsername = "hoteleposh@gmail.com";
                String smtpPassword = "tfymnzpuljryztjn";

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
                        mailMessage.Subject = "[Eposh Notifycation]";
                        mailMessage.IsBodyHtml = true;
                        mailMessage.Body =
           mailMessage.Body =
        $@"<!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>EPOSH-HOTEL Invoice</title>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    background-image: url('https://i.pinimg.com/736x/5d/98/8c/5d988c043c885fd910e0aedadbcfd423.jpg'); /* Đường dẫn đến hình ảnh nền */
                    background-size: cover;
                    background-repeat: no-repeat;
                    background-position: center center;
                    margin: 0;
                    padding: 0;
                }}
                .invoice-container {{
                    max-width: 600px;
                    margin: 20px auto;
                    background-color: rgba(255, 255, 255, 0.9);
                    border-radius: 8px;
                    padding: 20px;
                    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                }}
                .header {{
                    background-image:linear-gradient(to bottom right, #330867, #30CFD0);
                    color: #00A5F5;
                    padding: 10px;
                    border-radius: 8px 8px 0 0;
                    text-align: center;
                    
                }}
                .content {{
                    padding: 20px;
                    background-color: #C3CFE2
                    
                }}
                .content p{{
                    font-size:19px;
                    color:black;
                 }}
                .footer {{
                    background-image:linear-gradient(to bottom right, #330867, #30CFD0);
                    color: #fff;
                    padding: 10px;
                    border-radius: 0 0 8px 8px;
                    text-align: center;
                    font-szie: 19px;
                    font-weight:bold;
                }}
                .property {{
                    font-weight: bold;
                }}
                ul {{
                    list-style: none;
                    padding: 0;
                    font-size:18px;
                }}
                li {{
                    margin-bottom: 10px;
                    color:black;
                }}
                .property-name {{
                    font-weight: bold;
                    color: #007bff;
                }}  
                .logo {{
                    max-width: 120px; /* Kích thước tối đa của logo */
                }}
            </style>
        </head>
        <body>
            <div class='invoice-container'>
                <div class='header'>
                    <img class='logo' src='https://i.imgur.com/FrtuWsK.png' alt='EPOSH-HOTEL Logo' >
                </div>
                <div class='content'>
                    <ul>
                         <li><span class='property-name'>Notifycation:</span> {content}</li>
                    </ul>
                </div>
                <div class='footer'>
                    <p>Best Regards,</p>
                    <p>EPOSH-HOTEL Team</p>
                </div>
            </div>
        </body>
        </html>";

                        // Gửi email
                        client.Send(mailMessage);
                    }
                }

                return "Ok";

            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return ex.Message;
            }
        }

        public static double CheckRoomPrice(int roomID, DateTime CheckInDate, DateTime CheckOutDate)
        {
            DBContext db = new DBContext();
            var room = db.room
                          .Include(specialPrice => specialPrice.SpecialPrice)
                          .FirstOrDefault(room => room.RoomID == roomID);
            var specialPrice = room.SpecialPrice
                                   .FirstOrDefault(sp => CheckInDate >= sp.StartDate && CheckOutDate <= sp.EndDate
                                   || CheckInDate <= sp.EndDate);

            if (specialPrice != null)
            {
                return specialPrice.Price;
            }
            return room.Price;
        }
        public static String SendMailBooking([FromHeader] string toEmail,Booking booking)
        {
         
                int bookingDay = 0;
                double discountPrice = 0;
                double roomPrice = 0;
                double totalPrice = 0;
                double totalTaxesPrice = 0;
                double mainTotalPrice = 0;
                double totalPriceRoom = 0;
                double serviceFee = 0;
                bookingDay = (booking.CheckOutDate - booking.CheckInDate).Days;
                roomPrice = booking.UnitPrice;
                totalPrice = (roomPrice * booking.NumberOfRoom) * bookingDay;
                totalTaxesPrice = totalPrice * 0.05;
                
                totalPriceRoom = (roomPrice * booking.NumberOfRoom) * bookingDay;
                serviceFee = totalPrice * 0.1;
                mainTotalPrice = totalPrice + totalTaxesPrice + serviceFee;

            if (booking.Voucher != null && booking.Voucher.Discount > 0)
                {
                    discountPrice = (mainTotalPrice * booking.Voucher.Discount) / 100;
                }
                else
                {
                    discountPrice = 0;
                }
            string formattedRoomPrice = roomPrice.ToString("N0", new System.Globalization.CultureInfo("vi-VN"));
            string formattedTotalPriceRoom = totalPriceRoom.ToString("N0", new System.Globalization.CultureInfo("vi-VN"));
            string formattedTotalPrice = totalPrice.ToString("N0", new System.Globalization.CultureInfo("vi-VN"));
            string formattedTotalTaxesPrice = totalTaxesPrice.ToString("N0", new System.Globalization.CultureInfo("vi-VN"));
            string formattedDiscountPrice = discountPrice.ToString("N0", new System.Globalization.CultureInfo("vi-VN"));
            string formattedMainTotalPrice = booking.TotalPrice.ToString("N0", new System.Globalization.CultureInfo("vi-VN"));
            string formattedServiceFee = serviceFee.ToString("N0", new System.Globalization.CultureInfo("vi-VN"));
            // Cấu hình thông tin SMTP
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587; // Thay đổi nếu cần
            string smtpUsername = "hoteleposh@gmail.com";
            string smtpPassword = "tfymnzpuljryztjn";
            // URL cơ sở của máy chủ
            string baseUrl = "http://eposhgraduation-001-site1.ftempurl.com";
            string relativeImageUrl = booking.Room.Hotel.MainImage;
            string imageUrl = $"{baseUrl}{relativeImageUrl}";
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
                    mailMessage.Subject = "[Eposh Notifycation]";
                    mailMessage.IsBodyHtml = true;
                        mailMessage.Body =
       mailMessage.Body =
   $@"
            <!DOCTYPE html>
            <html lang=""en"">
   <html lang=""en"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Booking Confirmation</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        background-color: #f9f9f9;
                        margin: 0;
                        padding: 0;
                    }}
                    .container {{
                        background-color: #FFFAF0;
                        padding: 20px;
                        border-radius: 8px;
                        box-shadow: 0 0 10px rgba(0,0,0,0.1);
                        width: 100%;
                        max-width: 600px;
                        box-sizing: border-box;
                        margin: auto;
                    }}
                    .hotel-info {{
                        text-align: center;
                        margin-bottom: 20px;
                    }}
                    .hotel-image {{
                        width: 100%;
                        height: auto;
                        border-radius: 8px;
                    }}
                    .room-type {{
                        font-size: 18px;
                        font-weight: bold;
                    }}
                    .details, .cancellation {{
                        font-size: 14px;
                        color: #666;
                    }}
                    .cancellation span {{
                        color: #d9534f;
                    }}
                    .form-group {{
                        margin-bottom: 15px;
                    }}
                    .form-group label {{
                        display: block;
                        margin-bottom: 5px;
                    }}
                    .form-group input, .form-group select, .form-group button {{
                        width: 100%;
                        padding: 10px;
                        border: 1px solid #ccc;
                        border-radius: 4px;
                        box-sizing: border-box;
                    }}
                    .price-detail {{
                        text-align: right;
                        font-size: 14px;
                        color: #666;
                    }}
         
                    @media (max-width: 768px) {{
                        .price-detail {{
                            text-align: left;
                        }}
                    }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""hotel-info"">
                        <img src=""{imageUrl}"" alt=""Hotel Image"" class=""hotel-image"">
                        <h2>{booking.Room.Hotel.Name} - {booking.Room.Hotel.HotelAddress.Address}</h2>
                        <p class=""room-type"">{booking.Room.TypeOfRoom}</p>
                        <p class=""details"">{booking.NumberGuest} people &bull; {booking.Room.TypeOfBed} &bull; {booking.Room.SizeOfRoom} m²</p>
                        <p class=""cancellation"">Reservations can be canceled up to 24 hours before the check-in date</p>
                    </div>
                    <div class=""form-group"">
                        <label for=""checkin"">Check-in</label>
                        <input type=""date"" id=""checkin"" name=""checkin"" value=""{booking.CheckInDate:yyyy-MM-dd}"" disabled readonly required>
                    </div>
                   <div class=""form-group"">
                        <label for=""checkin"">Check-Out</label>
                        <input type=""date"" id=""checkin"" name=""checkin"" value=""{booking.CheckOutDate:yyyy-MM-dd}"" disabled readonly required>
                    </div>
                    <div class=""form-group"">
                        <label for=""fullname"">Fullname</label>
                        <input type=""text"" id=""fullname"" name=""fullname"" value=""{booking.Account.Profile.fullName}"" disabled readonly  required>
                    </div>
                    <div class=""form-group"">
                        <label for=""email"">Email</label>
                        <input type=""email"" id=""email"" name=""email"" value=""{booking.Account.Email}"" disabled readonly required>
                    </div>
                    <div class=""form-group"">
                        <label for=""phone"">Phone</label>  
                        <input type=""tel"" id=""phone"" name=""phone"" value=""{booking.Account.Phone}"" disabled readonly required>
                    </div>
                    <div class=""price-detail"">
                        <p>Room Price: <span>{formattedRoomPrice} VND</span></p>
                        <p>Total Night: <span>{bookingDay}</span></p>
                        <p>Number Of Room: <span>{booking.NumberOfRoom}</span></p>
                        <p>Total Price Room: <span>{formattedTotalPriceRoom} VND</span></p>
                        <p>Service Fee: <span>+{formattedServiceFee} VND</span></p>
                        <p>Taxes: <span>+{formattedTotalTaxesPrice} VND</span></p>
                        <p>Discount: <span>-{formattedDiscountPrice} VND</span></p>
                        <p>Total: <span>{formattedMainTotalPrice} VND</span></p>
                    </div>
                </div>
            </body>
            </html>";

                    // Gửi email
                    client.Send(mailMessage);
                }
            }

            return "Ok";
        }
    }
    /*
     * Step 1: Enable Google Drive API
     * Step 2: Create OAuth consent screen
     * Step 3: Credentials: Create Service Account
     * Step 4: Access Google Drive Share With Email created in Credentials
     * Step 5: Get ID Floder want save image
     * Step 6: Coppy Code and run
     */
    public class GoogleDriveService
    {
        private static readonly string[] Scopes = { DriveService.Scope.DriveFile };
        private static readonly string ApplicationName = "EposhBooking";
        private readonly string credentialsPath = "D:\\DoAnTotNghiep-FPTU-2024\\SourceCode\\EPOSHBOOKING_CapstoneProject\\EPOSHBOOKING_CapstoneProject\\GraduationAPI-EPOSHBOOKING\\service.json";

        public async Task<string> SaveImageToGoogleDrive(IFormFile image)
        {
            if (image == null)
            {
                throw new ArgumentNullException("Invalid image.");
            }

            // Load Service Account credentials
            GoogleCredential credential;
            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            // Initialize DriveService
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Create unique file name
            string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(image.FileName);

            // Define file metadata
            var fileMeta = new Google.Apis.Drive.v3.Data.File()
            {
                Name = uniqueFileName,
                MimeType = image.ContentType,
                Parents = new List<string> { "135gq12sGPYs4W-fdDUQI5-fbVol8kJez" }
            };

            // Upload file
            FilesResource.CreateMediaUpload request;
            using (var stream = new MemoryStream())
            {
                await image.CopyToAsync(stream);
                stream.Position = 0;
                request = service.Files.Create(fileMeta, stream, image.ContentType);
                request.Fields = "id";
                await request.UploadAsync();
            }

            var file = request.ResponseBody;
            return file != null ? $"https://drive.google.com/uc?id={file.Id}" : null!;
        }
    }


   

}