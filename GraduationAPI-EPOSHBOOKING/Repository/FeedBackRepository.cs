using GraduationAPI_EPOSHBOOKING.DataAccess;
using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using GraduationAPI_EPOSHBOOKING.Ultils;
using Microsoft.EntityFrameworkCore;
using System.Net;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Repository
{
    public class FeedBackRepository : IFeedbackRepository
    {
        private readonly DBContext db;
        private readonly IWebHostEnvironment environment;
        public FeedBackRepository(DBContext _db, IWebHostEnvironment environment)
        {
            this.db = _db;
            this.environment = environment;
        }
        public ResponseMessage CreateFeedBack(int BookingID, FeedBack feedBack, IFormFile Image)
        {
            try
            {
                var booking = db.booking.Include(room => room.Room).ThenInclude(hotel => hotel.Hotel).Include(account => account.Account)
                    .FirstOrDefault(booking => booking.BookingID == BookingID);
                if (booking == null)
                {
                    return new ResponseMessage { Success = false, Data = booking, Message = "Booking not found", StatusCode = (int)HttpStatusCode.NotFound };
                }
                if (environment == null)
                {
                    Console.WriteLine("Environment is null.");
                    return new ResponseMessage { Success = false, Data = null, Message = "Invalid environment settings.", StatusCode = (int)HttpStatusCode.BadRequest };
                }

                
                string savedImage = null;
                if (Image != null)
                {
                  
                    Console.WriteLine("Saving the image.");
                    savedImage = Ultils.Utils.SaveImage(Image, environment);
                }


                FeedBack addFeedBack = new FeedBack
                {
                    Account = booking.Account,
                    Booking = booking,
                    Rating = feedBack.Rating,
                    Description = feedBack.Description,
                    Hotel = booking.Room.Hotel,
                    Image = savedImage,
                    Status = "Normal"
                };

                db.feedback.Add(addFeedBack);
                db.SaveChanges();
                var result = new
                {
                    addFeedBack.FeedBackID,
                    addFeedBack.Rating,
                    addFeedBack.Description,
                    addFeedBack.Status,
                    addFeedBack.Image,
                    Account = addFeedBack.Account != null ? new
                    {
                        addFeedBack.Account.AccountID,
                        addFeedBack.Account.Email
                    } : null,
                    Booking = addFeedBack.Booking != null ? new
                    {
                        addFeedBack.Booking.BookingID,
                        addFeedBack.Booking.CheckInDate,
                        addFeedBack.Booking.CheckOutDate
                    } : null,
                    Hotel = addFeedBack.Hotel != null ? new
                    {
                        addFeedBack.Hotel.HotelID,
                        addFeedBack.Hotel.Name,
                        addFeedBack.Hotel.Description,
                        addFeedBack.Hotel.HotelStandar,
                        addFeedBack.Hotel.MainImage,

                        rooms = addFeedBack.Hotel.rooms?.Where(r => r != null).Select(r => new
                        {
                            r.RoomID,
                            r.TypeOfRoom,
                            r.Price,
                            SpecialPrices = r.SpecialPrice?.Where(sp => sp != null).Select(sp => new
                            {
                                sp.SpecialPriceID,
                                sp.Price,
                                sp.StartDate,
                                sp.EndDate
                            }).ToList()
                        }).ToList()
                    } : null
                };

                return new ResponseMessage { Success = true, Data = result, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };


            }
            catch (Exception ex)
            {
                return new ResponseMessage { Success = false, Data = null, Message = "Internal Server Error", StatusCode = (int)HttpStatusCode.InternalServerError };
            }
        }

        public ResponseMessage GetAllFeedbackHotel(int hotelID)
        {
            var listFeedback = db.feedback
                                 .Include(account => account.Account)
                                 .ThenInclude(profile => profile.Profile)
                                 .Where(feedback => feedback.Hotel.HotelID == hotelID && !feedback.Status.Equals("Hidden"))
                                 .ToList();

            return new ResponseMessage { Success = true,Data = listFeedback, Message = "Successfully", StatusCode= (int)HttpStatusCode.OK };
        }
    }
}
