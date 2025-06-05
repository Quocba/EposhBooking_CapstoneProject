using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using GraduationAPI_EPOSHBOOKING.DataAccess;
using GraduationAPI_EPOSHBOOKING.DesignPattern;
using GraduationAPI_EPOSHBOOKING.DTO;
using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.MiddleWare;
using GraduationAPI_EPOSHBOOKING.Model;
using GraduationAPI_EPOSHBOOKING.Model.Enum;
using GraduationAPI_EPOSHBOOKING.Ultils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;
using System.Text.RegularExpressions;
#pragma warning disable // turn of warming 

namespace GraduationAPI_EPOSHBOOKING.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly DBContext db;
        private readonly IWebHostEnvironment environment;
        private readonly IConfiguration configuration;
        private readonly CloudinaryService service;
        private readonly AccountSigleton accountSigleton;
        private readonly IMapper mapper;
        public AccountRepository(DBContext _db,IWebHostEnvironment _environment, IConfiguration configuration, CloudinaryService service,IMapper mapper)
        {
            this.db = _db;
            this.environment = _environment;
            this.configuration = configuration;
            this.service = service;
            accountSigleton = AccountSigleton.getInstance(db,mapper);
           
        }
        public ResponseMessage RegisterSigletance(RegisterDTO registerAccount)
        {
            accountSigleton.Register(registerAccount);
            var registeredAccount = accountSigleton.listAccount.LastOrDefault();
            return new ResponseMessage
            {
                Success = true,
                Message = "Success",
                Data = registerAccount,
                StatusCode = (int)HttpStatusCode.OK,
            };
        }
        public ResponseMessage RegisterPartnerAccount(Account account, String fullName)
        {
          
            var addRole = db.roles.FirstOrDefault(x => x.RoleID == (int)RoleEnum.Partner);
            var checkEmailAlready = db.accounts.FirstOrDefault(email => email.Email.Equals(account.Email));
            var checkPhone = db.accounts.FirstOrDefault(x => x.Phone.Equals(account.Phone));
            if (checkEmailAlready != null)
            {   
                return new ResponseMessage { Success = false, Data = checkEmailAlready.Email, Message = "Email is already exists. Please login!", StatusCode = (int)HttpStatusCode.AlreadyReported };
            }
           
            else if (checkPhone != null)
            {
                return new ResponseMessage { Success = false, Data = checkPhone.Phone, Message = "Phone is already exists. Please login!", StatusCode = (int)HttpStatusCode.AlreadyReported };
            }
            else
            {
                if (account != null && !fullName.IsNullOrEmpty())
                {
                    Model.Profile addProfile = new Model.Profile
                    {
                        fullName = fullName,
                    }; 
                    db.profiles.Add(addProfile);
                    Account addAccount = new Account
                    {
                        Email = account.Email,
                        Password = Ultils.Utils.HashPassword(account.Password),
                        Phone = account.Phone,
                        Role = addRole,
                        Profile = addProfile,
                        IsActive = false
                    };
                    db.accounts.Add(addAccount);
                    db.SaveChanges();
                    String otp = Ultils.Utils.sendMail(account.Email);
                    return new ResponseMessage { Success = true, Data = new { addAccount = addAccount, OTP = otp }, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
                }
                return new ResponseMessage { Success = false, Data = account, Message = "Register Fail", StatusCode = (int)HttpStatusCode.BadRequest };
            }

        }

        public ResponseMessage ActiveAccount(String email)
        {
            String emailParten = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
            Regex regex = new Regex(emailParten);
            if (regex.IsMatch(email))
            {
                var checkEmail = db.accounts.FirstOrDefault(x => x.Email.Equals(email));
                if (checkEmail != null)
                {
                    checkEmail.IsActive = true;
                    db.accounts.Update(checkEmail);
                    db.SaveChanges();
                    return new ResponseMessage { Success = true, Data = checkEmail, Message = "Your account has been activated", StatusCode = (int)HttpStatusCode.OK };
                }
                else
                {
                    return new ResponseMessage { Success = false, Data = checkEmail, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };
                }
            }
            return new ResponseMessage { Success = false, Data = email, Message = "Email is not in correct format. Please re-enter for example: Eposh@eposh.com" };
        }

        public ResponseMessage LoginWithNumberPhone(String phone)
        {
            string phoneRegex = @"^(?:\+84|0)([3|5|7|8|9])+([0-9]{8})$";
            Regex regex = new Regex(phoneRegex);
            string roleName = "Customer";
            var role = db.roles.FirstOrDefault(x => x.Name.Equals(roleName));

            if (regex.IsMatch(phone))
            {
                var checkPhone = db.accounts
                                   .Include(p => p.Profile)
                                   .Include(r => r.Role)
                                   .FirstOrDefault(x => x.Phone.Equals(phone));
                if (checkPhone != null)
                {
                    var responseData = new
                    {
                        AccountID = checkPhone.AccountID,
                        Email = checkPhone.Email ?? null,
                        Phone = checkPhone.Phone ?? null,
                        Role = checkPhone.Role?.Name ?? null,
                        FullName = checkPhone.Profile?.fullName ?? null,    
                        BirthDay = checkPhone.Profile?.BirthDay ?? null,
                        Gender = checkPhone.Profile?.Gender ?? null,
                        Address = checkPhone.Profile?.Address ?? null,
                        Avatar = checkPhone.Profile?.Avatar ?? null
                    };
                    var token = Ultils.Utils.CreateToken(checkPhone, configuration);
                    return new ResponseMessage { Success = true, Data = responseData, Token = token, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
                }
                if (checkPhone != null && checkPhone.IsActive == false)
                {
                    return new ResponseMessage { Success = false, Data = checkPhone, Token = "", Message = "Your account has been permanently blocked", StatusCode = (int)HttpStatusCode.Forbidden };
                }
                else
                {
                    Model.Profile addProfile = new Model.Profile
                    {
                        fullName = Ultils.Utils.GenerateRandomString()
                    };
                    db.profiles.Add(addProfile);
                    Account addAccount = new Account
                    {
                        Phone = phone,
                        Profile = addProfile,
                        IsActive = true,
                        Role = role
                    };
                    db.accounts.Add(addAccount);
                    db.SaveChanges();
                    var responseData = new
                    {
                        AccountID = addAccount.AccountID,
                        Email = addAccount.Email ?? null,
                        Phone = addAccount.Phone ?? null,
                        Role = addAccount.Role?.Name ?? null,
                        FullName = addAccount.Profile?.fullName ?? null,
                        BirthDay = addAccount.Profile?.BirthDay ?? null,
                        Gender = addAccount.Profile?.Gender ?? null,
                        Address = addAccount.Profile?.Address ?? null,
                        Avatar = addAccount.Profile?.Avatar ?? null
                    };
                    var token = Ultils.Utils.CreateToken(addAccount, configuration);
                    return new ResponseMessage { Success = true, Data = responseData, Token = token, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
                }
            }
            return new ResponseMessage { Success = false, Data = phone, Message = "Phone is not in correct format. Please re-enter for example: 0123456789", StatusCode = (int)HttpStatusCode.BadRequest };
        }
        public ResponseMessage Register(string email, string password, string fullName, string phone)
        {
            if (db.accounts.Any(a => a.Email == email))
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = "Email is already exists. Please login!",
                    StatusCode = (int)HttpStatusCode.AlreadyReported
                };  
            }
            var checkPhone = db.accounts.FirstOrDefault(x => x.Phone.Equals(phone));
            if (checkPhone != null) { 
                return new ResponseMessage { Success = false, Data = checkPhone.Email, Message = "Phone is already exists. Please login!", StatusCode = (int)HttpStatusCode.AlreadyReported };
            }

            string hashedPassword = Utils.HashPassword(password);
            var account = new Account
            {
                Email = email,
                Password = hashedPassword,
                IsActive = false,
                Phone = phone,
                Role = db.roles.FirstOrDefault(r => r.Name == "Customer")
            };
            var profile = new Model.Profile
            {
                fullName = fullName
            };

            account.Profile = profile;
            db.accounts.Add(account);
            db.SaveChanges();
            String otp = Ultils.Utils.sendMail(account.Email);
            return new ResponseMessage
            {
                Success = true,
                Message = "Registration Successfully",
                StatusCode = (int)HttpStatusCode.OK,
                Data = new {Account = account, otp = otp}
            };
        }

        public ResponseMessage UpdateNewPassword(string email, string newPassword)
        {
            var getAccount = db.accounts.FirstOrDefault(account => account.Email.Equals(email));
            if(getAccount != null)
            {
                getAccount.Password = Ultils.Utils.HashPassword(newPassword);
                db.accounts.Update(getAccount);
                db.SaveChanges();
                return new ResponseMessage { Success = true, Data = getAccount, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
                return new ResponseMessage { Success = false, Data = email, Message = "Email Does not exitst", StatusCode = (int)HttpStatusCode.NotFound };
           
        }

        public ResponseMessage ChangePassword(int accountId, string oldPassword, string newPassword)
        {
            var account = db.accounts.FirstOrDefault(a => a.AccountID == accountId);

            if (account == null)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = "Account not found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            string hashedOldPassword = Ultils.Utils.HashPassword(oldPassword);
            if (account.Password != hashedOldPassword)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = "Old password is incorrect",
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }

            string hashedNewPassword = Ultils.Utils.HashPassword(newPassword);
            account.Password = hashedNewPassword;

            db.SaveChanges();

            return new ResponseMessage
            {
                Success = true,
                Message = "Password changed successfully",
                Data  = account,
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        public async Task<ResponseMessage> UpdateProfileByAccount(int accountID,String email,String phone, Model.Profile profile,IFormFile avatar)
        {
            try
            {
                var getAccount = db.accounts.Include(p => p.Profile).FirstOrDefault(account => account.AccountID == accountID);
                if (getAccount == null)
                {
                    return new ResponseMessage { Success = false, Data = null, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };
                }
                if (avatar == null)
                {
                    getAccount.Email = getAccount.Email;
                    getAccount.Phone = phone;
                    getAccount.Profile.fullName = profile.fullName;
                    getAccount.Profile.BirthDay = profile.BirthDay;
                    getAccount.Profile.Gender = profile.Gender;
                    getAccount.Profile.Address = profile.Address;
                    getAccount.Profile.Avatar = profile.Avatar;
                    db.accounts.Update(getAccount);
                    db.SaveChanges();
                    return new ResponseMessage { Success = true, Data = getAccount, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
                }
                else
                {
                    getAccount.Email = getAccount.Email;
                    getAccount.Phone = phone;
                    getAccount.Profile.fullName = profile.fullName;
                    getAccount.Profile.BirthDay = profile.BirthDay;
                    getAccount.Profile.Gender = profile.Gender;
                    getAccount.Profile.Address = profile.Address;
                    getAccount.Profile.Avatar = await service.UploadImageAsync(avatar);
                    db.accounts.Update(getAccount);
                    db.SaveChanges();
                    return new ResponseMessage { Success = true, Data = getAccount, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };

                }

            }
            catch (Exception ex)
            {
                return new ResponseMessage { Success = false, Data = ex, Message = "Internal Server Error", StatusCode = (int)HttpStatusCode.InternalServerError };

            }

        }

        public ResponseMessage GetProfileByAccountId(int accountId)
        {
            var account = db.accounts
                            .Include(a => a.Profile)
                            .Include(Role => Role.Role)
                            .FirstOrDefault(a => a.AccountID == accountId);

            if (account == null)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = "Account not found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            if (account == null)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = "Profile not found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            return new ResponseMessage
            {
                Success = true,
                Data = account,
                Message = "Profile retrieved successfully",
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        public ResponseMessage GetAllAccount()
        {
            var listAccount = db.accounts.
                                Include(profile => profile.Profile)
                                .Include(role => role.Role)
                                .ToList();
            var result = listAccount.Where(x => !x.Role.Name.Equals("Admin")).Select(account => new
            {
               AccountID = account.AccountID,
               Email = account.Email,
               Phone = account.Phone,
               Role = account.Role,
               IsActive = account.IsActive,
               Profile = new Model.Profile
               {   ProfileID = account.Profile.ProfileID,
                   Address = account.Profile.Address,
                   BirthDay = account.Profile.BirthDay,
                   fullName = account.Profile.fullName,
                   Gender = account.Profile.Gender,
                   Avatar = account.Profile.Avatar,
               }
            });
            return new ResponseMessage { Success = true, Data =  result,Message = "Successfully",StatusCode = (int)HttpStatusCode.OK };
        }
            
        public ResponseMessage BlockedAccount(int accountID, String reasonBlock)
        {
            String reason = "Your EPOSH BOOKING account has been locked because " + reasonBlock;
            var getAccount = db.accounts.FirstOrDefault(account => account.AccountID == accountID);
            var getHotel = db.hotel.FirstOrDefault(hotel => hotel.Account.AccountID == accountID);
            if (getAccount != null && getHotel == null)
            {
                getAccount.IsActive = false;
                db.accounts.Update(getAccount);
                db.SaveChanges();
                Ultils.Utils.SendMailRegistration(getAccount.Email, reason);
                return new ResponseMessage { Success = true, Data = getAccount, Message = "Blocked Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
            if (getAccount != null && getHotel != null)
            {
                getAccount.IsActive = false;
                getHotel.Status = false;
                getHotel.isRegister = "Blocked";
                db.accounts.Update(getAccount);
                db.hotel.Update(getHotel);
                db.SaveChanges();
                Ultils.Utils.SendMailRegistration(getAccount.Email, reason);

                return new ResponseMessage { Success = true,Message = "Blocked Successfully", Data = getAccount, StatusCode = (int)HttpStatusCode.OK };
            }


            return new ResponseMessage { Success = false, Message = "Data not found", Data = new { account = getAccount, hotel = getHotel }, StatusCode = (int)HttpStatusCode.NotFound };
    
        }

        public ResponseMessage FilterAccountByStatus(bool isActice)
        {
            var filterAccount = db.accounts
                                  .Include(profile => profile.Profile)
                                  .Include(role => role.Role)
                                  .Where(account => account.IsActive ==  isActice)
                                  .ToList();
         
           return new ResponseMessage { Success = true, Data = filterAccount, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            

        }

        public ResponseMessage SearchAccountByName(string fullName)
        {
            var searchResult = db.accounts
                                 .Include(profile => profile.Profile)
                                 .Include(Role => Role.Role)
                                 .Where(account => account.Profile.fullName.Contains(fullName) && !account.Role.Name.Equals("Admin"))
                                 .ToList();
            var result = searchResult.Where(x => !x.Role.Name.Equals("Admin")).Select(account => new
            {
                AccountID = account.AccountID,
                Email = account.Email,
                Phone = account.Phone,
                Role = account.Role,
                IsActive = account.IsActive,
                Profile = new Model.Profile
                {
                    ProfileID = account.Profile.ProfileID,
                    Address = account.Profile.Address,
                    BirthDay = account.Profile.BirthDay,
                    fullName = account.Profile.fullName,
                    Gender = account.Profile.Gender,
                    Avatar = account.Profile.Avatar,
                }
            });

            return new ResponseMessage { Success = true, Data = result, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            
        }

        public ResponseMessage Login(String text, String password)
        {
            var passwordMD5 = Ultils.Utils.HashPassword(password);
            var checkAccount = db.accounts
                                 .Include(profile => profile.Profile)
                                 .Include(Role => Role.Role)
                                 .Include(hotel => hotel.Hotel)
                                 .FirstOrDefault(x => x.Email.Equals(text) && x.Password.Equals(passwordMD5) || x.Phone.Equals(text) && x.Password.Equals(passwordMD5));
           if(checkAccount != null && checkAccount.IsActive == true && checkAccount.Role.Name.Equals("Customer"))
            {
                var token = Ultils.Utils.CreateToken(checkAccount,configuration);
                return new ResponseMessage { Success = true, Data = checkAccount,Token = token, Message = "Successfully", StatusCode= (int)HttpStatusCode.OK };
            }
            if (checkAccount != null && checkAccount.IsActive == false && checkAccount.Role.Name.Equals("Customer"))
            {
                return new ResponseMessage { Success = false, Data = checkAccount, Message = "Your account has been permanently blocked.", StatusCode= (int)HttpStatusCode.Forbidden};

            }

            if (checkAccount != null && checkAccount.IsActive == true && checkAccount.Role.Name.Equals("Partner")
                && checkAccount.Hotel.Any(status => status.Status == true) && checkAccount.Hotel.Any(isRegister => isRegister.isRegister.Equals("Approved")))
            {   
                var token = Ultils.Utils.CreateToken(checkAccount, configuration);
                return new ResponseMessage { Success = true, Data = checkAccount, Token = token, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }

            if (checkAccount != null && checkAccount.IsActive == false && checkAccount.Role.Name.Equals("Partner")
              && checkAccount.Hotel.Any(status => status.Status == false) && checkAccount.Hotel.Any(isRegister => isRegister.isRegister.Equals("Blocked")))
            {
               return new ResponseMessage { Success = false, Data = checkAccount, Message = "Your account has been permanently blocked.", StatusCode = (int)HttpStatusCode.Forbidden };
            }
            
            if (checkAccount != null && checkAccount.IsActive == true && checkAccount.Role.Name.Equals("Partner")
                && checkAccount.Hotel.Any(status => status.Status == false) && checkAccount.Hotel.Any(isRegister => isRegister.isRegister.Equals("Awaiting Approval")))
            {
                return new ResponseMessage { 
                                            Success = true, 
                                            Data = checkAccount, 
                                            Message = "Your partner account is awaiting approval. Please wait for our response email.", 
                                            StatusCode = (int)(HttpStatusCode.Accepted) 
                                            };
            }
            if (checkAccount != null && checkAccount.IsActive == true && checkAccount.Role.Name.Equals("Partner")
             && !checkAccount.Hotel.Any())
            {
                return new ResponseMessage { 
                                            Success = false,
                                            Data = checkAccount, 
                                            Message = "Your account does not have any registered hotels.Please registered hotels.", 
                                            StatusCode = (int)HttpStatusCode.Created };
            }

            if (checkAccount != null && checkAccount.IsActive == true && checkAccount.Role.Name.Equals("Admin"))
            {
                var token = Ultils.Utils.CreateToken(checkAccount, configuration);
                return new ResponseMessage { Success = true, Data = checkAccount, Token = token, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }

            else
            {
                return new ResponseMessage { Success = false, Data = checkAccount, Message = "Login Fail", StatusCode = (int)(HttpStatusCode.NotFound) };
            }
            
        }

        public ResponseMessage GoogleLogin(string email, string userName, string avatar)
        {
            var check = db.accounts
                          .Include(role => role.Role)
                          .Include(hotel => hotel.Hotel)
                          .FirstOrDefault(x => x.Email.Equals(email));
            if (check != null && check.IsActive == true && check.Hotel.Any(h => h.Status == false) && check.Role.Name.Equals("Partner") &&
                check.Hotel.Any(h => h.isRegister.Equals("Awaiting Approval")))
            {
                return new ResponseMessage { Success = true, Data = check, Token = "", Message = "Your partner account is awaiting approval.",StatusCode =(int)HttpStatusCode.Accepted };
            }
            if (check != null && check.IsActive == false)
            {
                return new ResponseMessage { Success = false, Data = check, Token = "", Message = "Your account has been permanently blocked", StatusCode = (int)HttpStatusCode.Forbidden};
            }
            if (check != null && check.IsActive == true && check.Role.Name.Equals("Partner")
                 && !check.Hotel.Any())
            {
                return new ResponseMessage
                {
                    Success = false,
                    Data = check,
                    Message = "Your account does not have any registered hotels.Please registered hotels.",
                    StatusCode = (int)HttpStatusCode.Created
                };
            }

            if (check != null && check.Role.Name.Equals("Customer"))
            {
                var token = Ultils.Utils.CreateToken(check, configuration);
                return new ResponseMessage { Success = true, Data = check, Token = token, Message = "Login Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
            if (check != null && check.Hotel.Any(x => x.isRegister.Equals("Approved") && x.Status == true) && check.Role.Name.Equals("Partner"))
            {
                var token = Ultils.Utils.CreateToken(check, configuration);
                return new ResponseMessage { Success = true, Data = check, Token = token, Message = "Login Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
       

            else
            {
                String RoleName = "Customer";
                var role = db.roles.FirstOrDefault(x => x.Name.Equals(RoleName));
                Model.Profile createProfile = new Model.Profile
                {
                    fullName = userName,
                    Avatar = avatar
                };
                db.profiles.Add(createProfile);
                Account createAccount = new Account
                {
                    Email  = email,
                    Profile = createProfile,
                    IsActive = true,
                    Role = role
                };
                db.accounts.Add(createAccount);
                db.SaveChanges();
                var token = Ultils.Utils.CreateToken(createAccount, configuration);
                return new ResponseMessage { Success = true, Data = createAccount, Token = token, Message = "Login Successfully" ,StatusCode = (int)HttpStatusCode.OK};
            }
        }

        public ResponseMessage UpdateEmail(int accountID, string email)
        {
            var checkEmail = db.accounts.FirstOrDefault(x => x.Email.Equals(email));
            if (checkEmail == null)
            {
                var checkAccount = db.accounts
                             .Include(x => x.Role)
                             .Include(x => x.Profile)
                             .FirstOrDefault(x => x.AccountID == accountID);
                if (checkAccount != null)
                {
                    checkAccount.Email = email;
                    db.accounts.Update(checkAccount);
                    db.SaveChanges();
                    return new ResponseMessage { Success = true, Message = "Success", Data = checkAccount, StatusCode = (int)HttpStatusCode.OK };
                }     
            }
             return new ResponseMessage { Success = false, Message = "Email Already Exist", Data = checkEmail, StatusCode = (int)HttpStatusCode.AlreadyReported };
        }

        public ResponseMessage UpdatePhone(int accountID, string phone)
        {
            var checkPhone = db.accounts.FirstOrDefault(x => x.Phone.Equals(phone));
            if (checkPhone != null)
            {
                return new ResponseMessage { Success = false, Message = "Phone Already Exist", Data = checkPhone, StatusCode = (int)HttpStatusCode.AlreadyReported };

            }
            var checkAccount = db.accounts
                       .Include(x => x.Role)
                       .Include(x => x.Profile)
                       .FirstOrDefault(x => x.AccountID == accountID);
            if (checkAccount != null)
            {
                checkAccount.Phone = phone;
                db.accounts.Update(checkAccount);
                db.SaveChanges();
                return new ResponseMessage { Success = true, Message = "Success", Data = checkAccount, StatusCode = (int)HttpStatusCode.OK };
            }

            return new ResponseMessage { Success = false, Message = "Data Not Found", Data = checkAccount, StatusCode = (int)HttpStatusCode.NotFound };
        }

        public ResponseMessage GetAllAccountSigletacne()
        {
            var listAccount = accountSigleton.GetAllAccount();
            return new ResponseMessage
            {
                Success = true,
                Message = "Success",
                Data = listAccount,
                StatusCode = (int)HttpStatusCode.OK

            };
        }
    }
}
    