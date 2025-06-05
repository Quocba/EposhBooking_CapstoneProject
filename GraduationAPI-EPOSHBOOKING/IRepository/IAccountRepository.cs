using GraduationAPI_EPOSHBOOKING.DTO;
using GraduationAPI_EPOSHBOOKING.Model;

namespace GraduationAPI_EPOSHBOOKING.IRepository
{
    public interface IAccountRepository
    {
        ResponseMessage RegisterPartnerAccount(Account account,String fullName);
        ResponseMessage ActiveAccount(String email);
        ResponseMessage LoginWithNumberPhone(String phone);
        ResponseMessage Register(String email, String password, String fullName, String phone);
        ResponseMessage UpdateNewPassword(String email,String newPassword);
        //Task <ResponseMessage> UpdateProfileByAccount(int accountID,String email,String phone,Profile profile, IFormFile avatar);
        ResponseMessage GetProfileByAccountId(int accountId);
        ResponseMessage ChangePassword(int accountId, string oldPassword, string newPassword);
        ResponseMessage GetAllAccount();
        ResponseMessage BlockedAccount(int accountID,String reasonBlock);
        ResponseMessage FilterAccountByStatus(bool isActive);
        ResponseMessage SearchAccountByName(string fullName);
        ResponseMessage Login(String text, String password);
        ResponseMessage GoogleLogin(String email, String userName, String avatar);
        ResponseMessage UpdateEmail(int accountID, String email);
        ResponseMessage UpdatePhone(int accountID, String phone);
        ResponseMessage RegisterSigletance(RegisterDTO register);
        ResponseMessage GetAllAccountSigletacne();
        
    }
}
        
 

