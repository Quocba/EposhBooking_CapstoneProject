using AutoMapper;
using CloudinaryDotNet;
using GraduationAPI_EPOSHBOOKING.DataAccess;
using GraduationAPI_EPOSHBOOKING.DTO;
using GraduationAPI_EPOSHBOOKING.Model;

namespace GraduationAPI_EPOSHBOOKING.DesignPattern
{
    public class AccountSigleton
    {
        private readonly DBContext db;
        private static  AccountSigleton? instance;
        private readonly IMapper mapper;
        public GraduationAPI_EPOSHBOOKING.Model.Account account {  get; private set; }
        public List<GraduationAPI_EPOSHBOOKING.Model.Account> listAccount = new List<GraduationAPI_EPOSHBOOKING.Model.Account>();
        public AccountSigleton(DBContext _db, IMapper _mapper)
        {
            this.db = _db;
            account = new GraduationAPI_EPOSHBOOKING.Model.Account();
            this.mapper = _mapper;
            listAccount = db.accounts.ToList();
        }

        public static AccountSigleton getInstance(DBContext db, IMapper mapper) {
            if (instance == null) {
             
                instance = new AccountSigleton(db,mapper);
            }

            return instance;
        }

        public void Register(RegisterDTO registerDTO)
        {
            var account = mapper.Map<GraduationAPI_EPOSHBOOKING.Model.Account>(registerDTO);

            db.accounts.Add(account);
            db.SaveChanges();
            listAccount.Add(account);
        }

        public List<GraduationAPI_EPOSHBOOKING.Model.Account> GetAllAccount()
        {
            var listAccount = db.accounts.Where(x => !x.Email.Equals("AdminEposh@gmail.com")).ToList();
            return listAccount;
        }
        

    }
}
