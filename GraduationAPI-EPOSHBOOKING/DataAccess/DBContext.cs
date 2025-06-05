using GraduationAPI_EPOSHBOOKING.Model;
using Microsoft.EntityFrameworkCore;

namespace GraduationAPI_EPOSHBOOKING.DataAccess
{
    public class DBContext : DbContext
    {
        //public DBContext(DbContextOptions<DBContext> options) : base(options)
        //{
        //}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            //optionsBuilder.UseSqlServer("Data Source=SQL5106.site4now.net;User ID=db_aae0d9_eposhbooking_admin;Password=Admin123@;Trust Server Certificate=True");
            //optionsBuilder.UseSqlServer("Data Source=LAPTOP-3P8D5MG5;Initial Catalog=EPOSHGRADUATION;Integrated Security=True;Trust Server Certificate=True");
            //optionsBuilder.UseSqlServer("Data Source=LAPTOP-3P8D5MG5;Initial Catalog=EPOSHGraduation-Main;Integrated Security=True;Trust Server Certificate=True");
            optionsBuilder.UseSqlServer("Data Source=DESKTOP-B7FGES3;Initial Catalog=db_aaa284_eposhbooking;Integrated Security=True;Trust Server Certificate=True");
        }
        public DbSet<Account> accounts { get; set; }
        public DbSet<Role> roles { get; set; }
        public DbSet<Profile> profiles { get; set; }
        public DbSet<Blog> blog { get; set; }
        public DbSet<BlogImage> blogImage { get; set; }
        public DbSet<CommentBlog> blogComment { get; set; }
        public DbSet<Hotel> hotel { get; set; }
        public DbSet<HotelAddress> hotelAddress { get; set; }
        public DbSet<HotelImage> hotelImage { get; set; }
        public DbSet<HotelService> hotelService { get; set; }
        public DbSet<HotelSubService> hotelSubService { get; set;}
        public DbSet<Room> room { get; set; }
        public DbSet<RoomImage> roomImage { get; set; }
        public DbSet<RoomService> roomService { get; set; }
        public DbSet<RoomSubService> roomSubService { get; set; }
        public DbSet<Booking> booking { get; set; }
        public DbSet<FeedBack> feedback { get; set; }
        public DbSet<ReportFeedBack> reportFeedBack { get; set; }
        public DbSet<Voucher> voucher { get; set; }
        public DbSet<MyVoucher> myVoucher { get; set; }
        public DbSet<SpecialPrice> specialPrice { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyVoucher>().HasKey(o => new { o.VoucherID, o.AccountID });
        }

    }
}
