using GraduationAPI_EPOSHBOOKING.DataAccess;
using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Utils.Windows;
using System.Net;
using GraduationAPI_EPOSHBOOKING.Ultils;
using Microsoft.IdentityModel.Tokens;
using Azure;
using static System.Net.Mime.MediaTypeNames;
using GraduationAPI_EPOSHBOOKING.DTO;
using DocumentFormat.OpenXml.Office2013.Excel;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Globalization;
using System.Text;


#pragma warning disable // tắt cảnh báo để code sạch hơn
namespace GraduationAPI_EPOSHBOOKING.Repository
{
    public class HotelRepository : IHotelRepository
    {
        private readonly DBContext db;  
        private readonly Utils ultils;
        private readonly IWebHostEnvironment environment;
        public HotelRepository(DBContext _db, Utils _ultils, IWebHostEnvironment environment)
        {
            this.db = _db;
            this.ultils = _ultils;
            this.environment = environment;
        }   

        public ResponseMessage GetAllHotel()
        {
            var listHotel = db.hotel.Include(x => x.HotelAddress).Include(x => x.feedBacks)
                .Include(service => service.HotelServices)
                .ThenInclude(subService => subService.HotelSubServices)
                .Include(room => room.rooms).ThenInclude(x => x.SpecialPrice)
                .Where(hotel => hotel.Status == true && hotel.isRegister.Equals("Approved")).ToList();
                
            if (listHotel.Any())
            {
                var listHotelWithAvgRating = listHotel.Select(hotel => new
                {
                    HotelID = hotel.HotelID,
                    Name = hotel.Name,
                    MainImage = hotel.MainImage,
                    OpenedIn = hotel.OpenedIn,
                    Description = hotel.Description,
                    HotelStandar = hotel.HotelStandar,
                    IsRegister = hotel.isRegister,
                    Status = hotel.Status,
                    HotelAddress = new
                    {
                        hotel.HotelAddress.AddressID,
                        hotel.HotelAddress.Address,
                        hotel.HotelAddress.City,
                        hotel.HotelAddress.latitude,
                        hotel.HotelAddress.longitude
                    },
                    HotelService = hotel.HotelServices.Select(service => new
                    {
                        ServiceID = service.ServiceID,
                        ServiceType = service.Type,
                        HotelSubService = service.HotelSubServices.Select(subService => new
                        {
                            SubServiceID = subService.SubServiceID,
                            Name = subService.SubServiceName
                        }).ToList()
                    }).ToList(),

                    Rooms = hotel.rooms
                        .Where(room => room != null && room.Quantity > 0 ) // Lọc giá trị null trong rooms
                        .Select(room => 
                        {
                            var currentDate = DateTime.Now.AddHours(14);
                            var specialPrice = room.SpecialPrice
                                .FirstOrDefault(sp => currentDate.Date >= sp.StartDate && currentDate.Date <= sp.EndDate);
                            if (specialPrice != null)
                            {
                                room.Price = specialPrice.Price;
                            }   
                            return new
                            {
                                RoomID = room.RoomID,
                                TypeOfRoom = room.TypeOfRoom,
                                NumberCapacity = room.NumberCapacity,
                                Price = room.Price,
                                Quantity = room.Quantity,
                                SizeOfRoom = room.SizeOfRoom,
                                TypeOfBed = room.TypeOfBed,
                                SpecialPrice = room.SpecialPrice.Select(sp => new
                                {
                                    SpecialPriceID = sp.SpecialPriceID,
                                    StartDate = sp.StartDate,
                                    EndDate = sp.EndDate,
                                    Price = sp.Price
                                }).ToList(),
                            };
                        }).OrderBy(x => x.Price).ToList(),
                    FeedBack = hotel.feedBacks.Select(feedback => new   
                    {
                        FeedBackID = feedback.FeedBackID,
                        Rating = feedback.Rating,
                        Image = feedback.Image,
                        Description = feedback.Description,
                        Status = feedback.Status,
                        Account = feedback.Account
                    }).Where(x => !x.Status.Equals("Hidden")).ToList(),
                    AvgRating = hotel.feedBacks.Where(x => !x.Status.Equals("Hidden")).Any() 
                                ? Math.Round(hotel.feedBacks.Where(x => !x.Status.Equals("Hidden")).Average(feedback => feedback.Rating), 1) : 0,
                }).ToList();


                return new ResponseMessage { Success = true, Data = listHotelWithAvgRating, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
            else
            {
                return new ResponseMessage { Success = true, Data = listHotel, Message = "No Data", StatusCode = (int)HttpStatusCode.NotFound };
            }
        }

        public ResponseMessage GetHotelByCity(String city)
        {
            var getListHotel = db.hotel
                .Include(x => x.HotelAddress)
                .Include(x => x.feedBacks)
                .Include(room => room.rooms)
                .ThenInclude(specialPrice => specialPrice.SpecialPrice)
                .OrderByDescending(hotel => hotel.HotelStandar)
                .Where(x => x.HotelAddress.City.Equals(city) && x.Status == true && x.isRegister.Equals("Approved")).ToList();

            if (getListHotel.Any())
            {
                var listHotelWithAvgRating = getListHotel.Select(hotel => new
                {
                    Hotel = hotel,
                    Avgrating = hotel.feedBacks.Any() ? hotel.feedBacks.Average(feedBack => feedBack.Rating) : 0,
                    Room = hotel.rooms.Select(room =>
                    {
                        var currentDate = DateTime.Now.AddHours(14);
                        var specialPrice = room.SpecialPrice
                                               .FirstOrDefault(sp => currentDate.Date >= sp.StartDate && currentDate.Date <= sp.EndDate);
                        if (specialPrice != null)
                        {
                            room.Price = specialPrice.Price;
                        }
                        return new
                        {
                            RoomID = room.RoomID,
                            TypeOfRoom = room.TypeOfRoom,
                            NumberCapacity = room.NumberCapacity,
                            Price = room.Price,
                            Quantity = room.Quantity,
                            SizeOfRoom = room.SizeOfRoom,
                            TypeOfBed = room.TypeOfBed,
                            SpecialPrice = room.SpecialPrice.Select(sp => new
                            {
                                SpecialPriceID = sp.SpecialPriceID,
                                StartDate = sp.StartDate,
                                EndDate = sp.EndDate,
                                Price = sp.Price
                            }).ToList()
                        };
                    }).ToList()
                }).Select(hotel => new
                {
                    HotelID = hotel.Hotel.HotelID,
                    MainImage = hotel.Hotel.MainImage,
                    Name = hotel.Hotel.Name,
                    OpenedIn = hotel.Hotel.OpenedIn,
                    Description = hotel.Hotel.Description,
                    HotelStandar = hotel.Hotel.HotelStandar,
                    IsRegister = hotel.Hotel.isRegister,
                    Status = hotel.Hotel.Status,
                    HotelAddress = hotel.Hotel.HotelAddress,
                    FeedBack = hotel.Hotel.feedBacks.Select(feedback => new
                    {
                        FeedBackID = feedback.FeedBackID,
                        Rating = feedback.Rating,
                        Image = feedback.Image,
                        Description = feedback.Description,
                        isDelete = feedback.Status,
                        Account = feedback.Account
                    }),
                    Rooms = hotel.Room,
                    AvgRating = hotel.Avgrating
                }).ToList();
                return new ResponseMessage { Success = true, Data = listHotelWithAvgRating, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
            else
            {
                return new ResponseMessage { Success = false, Data = getListHotel, Message = "No Data", StatusCode = (int)HttpStatusCode.NotFound };
            }
        }

        public ResponseMessage GetHotelByID(int id)
        {
            var getHotel = db.hotel
                .Include(image => image.HotelImages)
                .Include(address => address.HotelAddress)
                .Include(room => room.rooms)
                .ThenInclude(specialPrice => specialPrice.SpecialPrice)
                .FirstOrDefault(x => x.HotelID == id && x.Status == true && x.isRegister.Equals("Approved"));
            
   
            if (getHotel != null)
            {
                var currentDate = DateTime.Now.AddHours(14);

                var roomsWithUpdatedPrice = getHotel.rooms.Select(room =>
                {
                    var specialPrice = room.SpecialPrice.FirstOrDefault(sp => currentDate.Date >= sp.StartDate.Date && currentDate <= sp.EndDate);
                    if (specialPrice != null)
                    {
                        room.Price = specialPrice.Price;
                    }
                    return room;
                }).ToList();
                var totalQuantity = getHotel?.rooms.Sum(room => room.Quantity) ?? 0;
                var responseData = new
                {
                    Hotel = getHotel,
                    TotalRoom = totalQuantity
                };

                return new ResponseMessage { Success = true, Data = responseData, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
            else
            {

                return new ResponseMessage { Success = false, Data = getHotel, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };
            }
        }


        public ResponseMessage GetHotelByPrice(String city, double minPrice, double maxPrice)
        {
            var currentDate = DateTime.Now.AddHours(14);
            var getHotel = db.hotel
                             .Include(x => x.HotelAddress)
                             .Include(x => x.feedBacks)
                             .Include(x => x.rooms)
                             .ThenInclude(x => x.SpecialPrice)
                             .OrderByDescending(hotel => hotel.HotelStandar)
                             .Where(hotel => hotel.HotelAddress.City.Equals(city))
                             .ToList();
            var filterHotel = getHotel.Select(hotel => new
            {
                HotelID = hotel.HotelID,
                Name = hotel.Name,
                Description = hotel.Description,
                HotelStandar = hotel.HotelStandar,
                MainImage = hotel.MainImage,
                OpenedIn = hotel.OpenedIn,
                HotelAddress = new
                {
                    hotel.HotelAddress.AddressID,
                    hotel.HotelAddress.Address,
                    hotel.HotelAddress.City,
                    hotel.HotelAddress.latitude,
                    hotel.HotelAddress.longitude
                },
                Rooms = hotel.rooms.Select(room =>
                {
                    var finalPrice = room.Price;
                    var specialPrice = room.SpecialPrice
                        .FirstOrDefault(sp => currentDate.Date >= sp.StartDate && currentDate.Date <= sp.EndDate);
                    if (specialPrice != null)
                    {
                        finalPrice = specialPrice.Price;
                    }
                    return new
                    {
                        RoomID = room.RoomID,
                        TypeOfRoom = room.TypeOfRoom,
                        NumberCapacity = room.NumberCapacity,
                        Price = finalPrice,
                        Quantity = room.Quantity,
                        SizeOfRoom = room.SizeOfRoom,
                        TypeOfBed = room.TypeOfBed,
                        SpecialPrice = room.SpecialPrice.Select(sp => new
                        {
                            SpecialPriceID = sp.SpecialPriceID,
                            StartDate = sp.StartDate,
                            EndDate = sp.EndDate,
                            Price = sp.Price
                        }).ToList()
                    };
                }).Where(room => room.Price >= minPrice && room.Price <= maxPrice).ToList(),
                FeedBacks = hotel.feedBacks.Select(feedback => new
                {
                    FeedBackID = feedback.FeedBackID,
                    Rating = feedback.Rating,
                    Image = feedback.Image,
                    Description = feedback.Description,
                    isDelete = feedback.Status,
                    Account = feedback.Account
                }).ToList()
            }).Where(hotel => hotel.Rooms.Any()).ToList();


            var listHotelWithRating = filterHotel.Select(hotel => new
            {
                Hotel = hotel,
                AvgRating = hotel.FeedBacks.Any() ? Math.Round(hotel.FeedBacks.Average(feedback => feedback.Rating), 1) : 0,
                Count = hotel.FeedBacks.Count()
            });

            if (listHotelWithRating.Any())
            {
                return new ResponseMessage { Success = true, Data = listHotelWithRating, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
            return new ResponseMessage { Success = false, Data = listHotelWithRating, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };
        }

        public ResponseMessage GetByHotelStandar(int hotelStandar)
        {
            var currentDate = DateTime.Now.AddHours(14);

            var listHotel = db.hotel
                                    .Include(x => x.HotelAddress)
                                    .Include(x => x.feedBacks)
                                    .Include(x => x.rooms)
                                    .ThenInclude(x => x.SpecialPrice)
                                    .Where(hotel => hotel.Status == true && hotel.isRegister.Equals("Approved"))
                                    .ToList();

            var filterHotelWithRating = listHotel.OrderByDescending(hotel => hotel.HotelStandar)
                                                  .Select(hotel => new
                                                  {
                                                      HotelID = hotel.HotelID,
                                                      Name = hotel.Name,
                                                      Description = hotel.Description,
                                                      HotelAddress = hotel.HotelAddress == null ? null : new
                                                      {
                                                          hotel.HotelAddress.AddressID,
                                                          hotel.HotelAddress.Address,
                                                          hotel.HotelAddress.City,
                                                          hotel.HotelAddress.latitude,
                                                          hotel.HotelAddress.longitude
                                                      },
                                                      HotelStandar = hotel.HotelStandar,
                                                      MainImage = hotel.MainImage,
                                                      OpenedIn = hotel.OpenedIn,
                                                      HotelServices = hotel.HotelServices?.Select(service => new
                                                      {
                                                          service.ServiceID,
                                                          service.Type,
                                                          HotelSubServices = service.HotelSubServices?.Select(subService => new
                                                          {
                                                              subService.SubServiceID,
                                                              subService.SubServiceName
                                                          }).ToList()
                                                      }).ToList(),
                                                      Rooms = hotel.rooms?.Select(room =>
                                                      {
                                                          var specialPrice = room.SpecialPrice
                                                              .FirstOrDefault(sp => currentDate.Date >= sp.StartDate && currentDate.Date <= sp.EndDate);
                                                          return new
                                                          {
                                                              RoomID = room.RoomID,
                                                              TypeOfRoom = room.TypeOfRoom,
                                                              NumberCapacity = room.NumberCapacity,
                                                              Price = specialPrice?.Price ?? room.Price,
                                                              Quantity = room.Quantity,
                                                              SizeOfRoom = room.SizeOfRoom,
                                                              TypeOfBed = room.TypeOfBed,
                                                              SpecialPrice = room.SpecialPrice?.Select(sp => new
                                                              {
                                                                  sp.SpecialPriceID,
                                                                  sp.StartDate,
                                                                  sp.EndDate,
                                                                  sp.Price
                                                              }).ToList()
                                                          };
                                                      }).ToList(),
                                                      AvgRating = hotel.feedBacks != null && hotel.feedBacks.Any() ?
                                                                  Math.Round(hotel.feedBacks.Average(fb => fb.Rating), 1) : 0,
                                                      Count = hotel.feedBacks.Count()
                                                  });

            var getHotelWithHotelStandar = filterHotelWithRating.Where(hotel => hotel.HotelStandar <= hotelStandar)
                                                                .OrderByDescending(hotel => hotel.HotelStandar)
                                                                .ToList();

            if (getHotelWithHotelStandar.Any())
            {
                return new ResponseMessage { Success = true, Data = getHotelWithHotelStandar, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
            else
            {
                return new ResponseMessage { Success = false, Data = getHotelWithHotelStandar, Message = "No Data", StatusCode = (int)HttpStatusCode.NotFound };
            }
        }


        public ResponseMessage GetByService(List<String> services)
        {
            var currentDate = DateTime.Now.AddHours(14);
            if (services.Any())
            {
                var listHotel = db.hotel
                                        .Include(x => x.HotelAddress)
                                        .Include(x => x.HotelServices).ThenInclude(x => x.HotelSubServices)
                                        .Include(x => x.feedBacks)
                                        .Include(x => x.rooms).ThenInclude(special => special.SpecialPrice)
                                        .Where(hotel => hotel.Status == true && hotel.isRegister.Equals("Approved"))
                                        .ToList();


                var listHotelWithService = listHotel.Where(hotel => hotel.HotelServices.Any(service => services.Contains(service.Type)))
                                                    .OrderByDescending(hotel => hotel.HotelStandar)
                                                    .ToList();


                var result = listHotelWithService.Select(hotel => new
                {
                    HotelID = hotel.HotelID,
                    Name = hotel.Name,
                    Description = hotel.Description,
                    HotelAddress = new
                    {
                        hotel.HotelAddress.AddressID,
                        hotel.HotelAddress.Address,
                        hotel.HotelAddress.City,
                        hotel.HotelAddress.latitude,
                        hotel.HotelAddress.longitude
                    },
                    HotelStandar = hotel.HotelStandar,
                    MainImage = hotel.MainImage,
                    OpenedIn = hotel.OpenedIn,
                    HotelImages = hotel.HotelImages.Select(img => new
                    {
                        img.ImageID,
                        img.Image,

                    }).ToList(),
                    HotelServices = hotel.HotelServices.Select(service => new
                    {
                        service.ServiceID,
                        service.Type,
                        HotelSubServices = service.HotelSubServices.Select(subService => new
                        {
                            subService.SubServiceID,
                            subService.SubServiceName
                        }).ToList()
                    }).ToList(),
                    FeedBacks = hotel.feedBacks.Select(feedback => new
                    {
                        feedback.FeedBackID,
                        feedback.Rating,
                        feedback.Image,
                        feedback.Description,
                        feedback.Status,
                        feedback.Account
                    }).ToList(),
                    Rooms = hotel.rooms.Select(room =>
                    {
                        var specialPrice = room.SpecialPrice
                            .FirstOrDefault(sp => currentDate.Date >= sp.StartDate && currentDate.Date <= sp.EndDate);
                        return new
                        {
                            RoomID = room.RoomID,
                            TypeOfRoom = room.TypeOfRoom,
                            NumberCapacity = room.NumberCapacity,
                            Price = specialPrice?.Price ?? room.Price,
                            Quantity = room.Quantity,
                            SizeOfRoom = room.SizeOfRoom,
                            TypeOfBed = room.TypeOfBed,
                            SpecialPrice = room.SpecialPrice.Select(sp => new
                            {
                                sp.SpecialPriceID,
                                sp.StartDate,
                                sp.EndDate,
                                sp.Price
                            }).ToList()
                        };

                    }).ToList(),
                    AvgRating = hotel.feedBacks != null && hotel.feedBacks.Any() ? Math.Round(hotel.feedBacks.Average(fb => fb.Rating), 1) : 0,
                    Count = hotel.feedBacks.Count()
                }).ToList();

                if (result.Any())
                {
                    return new ResponseMessage { Success = true, Data = result, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
                }
                else
                {
                    return new ResponseMessage { Success = false, Data = null, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };
                }
            }
            else
            {
                return new ResponseMessage { Success = false, Data = null, Message = "No services provided", StatusCode = (int)HttpStatusCode.BadRequest };
            }
        }


        public ResponseMessage GetServiceByHotelID(int hotelID)
        {
            var hotelService = db.hotelService
                                 .Include(hotel => hotel.Hotel)
                                 .Include(subService => subService.HotelSubServices)
                                 .Where(hotel => hotel.Hotel.HotelID == hotelID && hotel.Hotel.Status == true && hotel.Hotel.isRegister.Equals("Approved"))
                                 .ToList()
                                 .Select(service => new
                                 {
                                     ServiceID = service.ServiceID,
                                     Type = service.Type,
                                     SubServices = service.HotelSubServices.Select(subService => new
                                     {
                                         SubServiceID = subService.SubServiceID,
                                         SubServiceName = subService.SubServiceName
                                     }).ToList()
                                 });
            if (hotelService.Any())
            {
                return new ResponseMessage { Success = true, Data = hotelService, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
            else
            {
                return new ResponseMessage { Success = false, Data = hotelService, Message = "No Data", StatusCode = (int)HttpStatusCode.NotFound };
            }
        }


        public ResponseMessage GetGalleriesByHotelID(int hotelID)
        {
            var getGalleries = db.hotel.Include(x => x.HotelImages).Where(hotel => hotel.HotelID == hotelID).SelectMany(hotel => hotel.HotelImages).ToList();
            var responseData = getGalleries.Select(image => new
            {
                Image = new HotelImage
                {
                    ImageID = image.ImageID,
                    Image = image.Image // Assuming HotelImage has ImageUrl property
                }
            }).ToList();
            if (getGalleries.Any())
            {
                return new ResponseMessage { Success = true, Data = getGalleries, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
            else
            {
                return new ResponseMessage { Success = false, Data = getGalleries, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };
            }
        }

       

  


        public ResponseMessage HotelRegistration(HotelRegistrationDTO registration, List<ServiceTypeDTO> Services)
        {
            var account = db.accounts
                         .Include(profile => profile.Profile)
                         .FirstOrDefault(a => a.AccountID == registration.AccountID);

            var addAddress = new HotelAddress
            {
                Address = Ultils.Utils.RemoveVietnameseDiacritics(registration.HotelAddress),
                City = Ultils.Utils.RemoveVietnameseDiacritics(registration.City),
                latitude = registration.Latitude,
                longitude = registration.Longitude
            };

            db.hotelAddress.Add(addAddress);

            var addHotel = new Hotel
            {
                Name = registration.HotelName,
                OpenedIn = registration.OpenedIn,
                MainImage = Utils.SaveImage(registration.MainImage, environment),
                Description = registration.Description,
                HotelAddress = addAddress,
                Status = false,
                isRegister = "Awaiting Approval",
                HotelStandar = 0,
                Account = account
            };

            db.hotel.Add(addHotel);
            db.SaveChanges(); // Save changes to generate IDs for the hotel


            foreach (var service in Services)
            {
                var addService = new HotelService
                {
                    Type = service.serviceType,
                    Hotel = addHotel,

                };
                db.hotelService.Add(addService);
                db.SaveChanges();
                var hotelSubService = new List<HotelSubService>();
                foreach (var subServiceName in service.subServiceName)
                {
                    var addSubService = new HotelSubService
                    {
                        SubServiceName = subServiceName,
                        HotelService = addService
                    };
                    db.hotelSubService.Add(addSubService);
                    hotelSubService.Add(addSubService);
                }
                addService.HotelSubServices = hotelSubService;

            }

            foreach (var img in registration.Images)
            {
                var addImage = new HotelImage
                {
                    Image = Ultils.Utils.SaveImage(img, environment),
                    Hotel = addHotel,
                    Title = "Hotel View"
                };

                db.hotelImage.Add(addImage);
            }
            var responseData = new
            {
                HotelID = addHotel.HotelID,
                MainImage = addHotel.MainImage,
                Name = addHotel.Name,
                OpenedIn = addHotel.OpenedIn,
                Description = addHotel.Description,
                HotelStandar = addHotel.HotelStandar,
                IsRegister = addHotel.isRegister,
                Status = addHotel.Status,
                Account = new
                {
                    addHotel.Account.AccountID,
                    addHotel.Account.Email,
                    addHotel.Account.Phone,
                    addHotel.Account.IsActive,
                    Profile = new
                    {
                        addHotel.Account.Profile.ProfileID,
                        addHotel.Account.Profile.fullName,
                        addHotel.Account.Profile.BirthDay,
                        addHotel.Account.Profile.Gender,
                        addHotel.Account.Profile.Address,
                        addHotel.Account.Profile.Avatar
                    }
                },
                HotelAddress = new
                {
                    addHotel.HotelAddress.AddressID,
                    addHotel.HotelAddress.Address,
                    addHotel.HotelAddress.City,
                    addHotel.HotelAddress.latitude,
                    addHotel.HotelAddress.longitude
                },
                HotelImages = addHotel.HotelImages.Select(img => new
                {
                    img.ImageID,
                    img.Title,
                    img.Image
                }).ToList(),
                HotelServices = addHotel.HotelServices.Select(service => new
                {
                    service.ServiceID,
                    service.Type,
                    HotelSubServices = service.HotelSubServices.Select(subService => new
                    {
                        subService.SubServiceID,
                        subService.SubServiceName
                    }).ToList()
                }).ToList()
            };
            db.SaveChanges(); // Save all changes at the end
            return new ResponseMessage
            {
                Success = true,
                Data = responseData,
                Message = "Successfully registered hotel",
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        public ResponseMessage UpdateBasicInformation(int hotelID, string hotelName, int openedIn, string description, IFormFile mainImage)
        {
            var getHotel = db.hotel.FirstOrDefault(hotel => hotel.HotelID == hotelID);
            if (getHotel != null)
            {
                if (mainImage != null)
                {
                    getHotel.Name = hotelName;
                    getHotel.OpenedIn = openedIn;
                    getHotel.Description = description;
                    getHotel.HotelStandar = getHotel.HotelStandar;
                    getHotel.MainImage = Utils.SaveImage(mainImage, environment);
                    db.hotel.Update(getHotel);
                    db.SaveChanges();
                    return new ResponseMessage { Success = true, Data = getHotel, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
                }
                else
                {
                    getHotel.Name = hotelName;
                    getHotel.OpenedIn = openedIn;
                    getHotel.Description = description;
                    getHotel.HotelStandar = getHotel.HotelStandar;
                    getHotel.MainImage = getHotel.MainImage;
                    db.hotel.Update(getHotel);
                    db.SaveChanges();
                    return new ResponseMessage { Success = true, Data = getHotel, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
                }


            }
            return new ResponseMessage { Success = false, Data = getHotel, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };
        }

        public ResponseMessage UpdateHotelService(int hotelID, List<ServiceTypeDTO> services)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Find the hotel
                    var hotel = db.hotel.Include(h => h.HotelServices)
                                        .ThenInclude(s => s.HotelSubServices)
                                        .FirstOrDefault(h => h.HotelID == hotelID);

                    if (hotel == null)
                    {
                        return new ResponseMessage
                        {
                            Success = false,
                            Message = "Hotel not found",
                            StatusCode = (int)HttpStatusCode.NotFound
                        };
                    }

                    // Remove existing services and sub-services
                    foreach (var service in hotel.HotelServices)
                    {
                        db.hotelSubService.RemoveRange(service.HotelSubServices);
                        db.hotelService.Remove(service);
                    }
                    db.SaveChanges();

                    // Add new services and sub-services
                    foreach (var serviceType in services)
                    {
                        var addService = new HotelService
                        {
                            Type = serviceType.serviceType,
                            Hotel = hotel // Associate the service with the hotel
                        };

                        db.hotelService.Add(addService);
                        db.SaveChanges(); // Save changes to generate IDs for the service

                        var hotelSubServices = new List<HotelSubService>();

                        foreach (var subServiceName in serviceType.subServiceName)
                        {
                            var addSubService = new HotelSubService
                            {
                                SubServiceName = subServiceName,
                                HotelService = addService // Use the generated ServiceID
                            };

                            db.hotelSubService.Add(addSubService);
                            hotelSubServices.Add(addSubService);
                        }

                        addService.HotelSubServices = hotelSubServices;
                    }

                    db.SaveChanges(); // Save all changes at the end
                    transaction.Commit();

                    return new ResponseMessage
                    {
                        Success = true,
                        Data = hotel,
                        Message = "Successfully updated hotel services",
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    return new ResponseMessage
                    {
                        Success = false,
                        Message = ex.Message,
                        StatusCode = (int)HttpStatusCode.InternalServerError
                    };
                }
            }
        }

        public ResponseMessage AddHotelImage(int hotelId, String title, IFormFile images)
        {
            var hotel = db.hotel.FirstOrDefault(hotel => hotel.HotelID == hotelId);
            if (hotel != null)
            {
                HotelImage addImage = new HotelImage
                {
                    Title = title,
                    Image = Ultils.Utils.SaveImage(images, environment),
                    Hotel = hotel
                };
                db.hotelImage.Add(addImage);
                db.SaveChanges();
                return new ResponseMessage { Success = true, Data = hotel, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
            return new ResponseMessage { Success = false, Data = null, Message = "Hotel not found", StatusCode = (int)HttpStatusCode.NotFound };
        }
        public ResponseMessage DeleteHotelImages(int imageID)
        {
            var getImage = db.hotelImage.FirstOrDefault(image => image.ImageID == imageID);
            if (getImage != null)
            {
                db.hotelImage.Remove(getImage);
                db.SaveChanges();
                return new ResponseMessage { Success = true, Data = getImage, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
            return new ResponseMessage { Success = false, Data = null, Message = "Image not found", StatusCode = (int)HttpStatusCode.NotFound };
        }

        public ResponseMessage GetAllHotelInfomation()
        {
            var listHotel = db.hotel
                              .Include(feedbacks => feedbacks.feedBacks)
                              .Include(hotelService => hotelService.HotelServices)
                              .ThenInclude(hotelSubService => hotelSubService.HotelSubServices)
                              .Include(address => address.HotelAddress)
                              .Include(account => account.Account)
                              .ThenInclude(profile => profile.Profile)
                              .ToList()
                              .Select(hotel => new
                              {
                                  HotelID = hotel.HotelID,
                                  MainImage = hotel.MainImage,
                                  Name = hotel.Name,
                                  OpenedIn = hotel.OpenedIn,
                                  Description = hotel.Description,
                                  HotelStandar = hotel.HotelStandar,
                                  IsRegister = hotel.isRegister,
                                  Status = hotel.Status,
                                  Account = new
                                  {
                                      AccountID = hotel.Account.AccountID,
                                      Email = hotel.Account.Email,
                                      Password = hotel.Account.Password,
                                      Phone = hotel.Account.Phone,
                                      IsActive = hotel.Account.IsActive,
                                      Profile = new
                                      {
                                          ProfileID = hotel.Account.Profile.ProfileID,
                                          FullName = hotel.Account.Profile.fullName,
                                          BirthDay = hotel.Account.Profile.BirthDay,
                                          Gender = hotel.Account.Profile.Gender,
                                          Avatar = hotel.Account.Profile.Avatar
                                      }
                                  },
                                  HotelAddress = hotel.HotelAddress,
                                  HotelService = hotel.HotelServices.Select(service => new
                                  {
                                      ServiceID = service.ServiceID,
                                      Type = service.Type,
                                      HotelSubService = service.HotelSubServices.Select(sub => new
                                      {
                                          SubServiceID = sub.SubServiceID,
                                          SubServiceName = sub.SubServiceName
                                      })
           
                                  }),
                                  FeedBack = hotel.feedBacks.Where(x => !x.Status.Equals("Hidden")).Select(feedbacks => new
                                  {
                                      FeedbackID = feedbacks.FeedBackID,
                                      Rating = feedbacks.Rating,
                                      Image = feedbacks.Image,
                                      Description = feedbacks.Description,
                                      Status = feedbacks.Status
                                  }),
                                  TotalBooking = db.booking.Count(booking => booking.Room.Hotel.HotelID == hotel.HotelID),
                                  TotalRevenue = db.booking.Where(booking => booking.Room.Hotel.HotelID == hotel.HotelID).Sum(booking => booking.TotalPrice),
                                  AvgRating = hotel.feedBacks.Where(x => !x.Status.Equals("Hidden")).Any()
                                               ? Math.Round(hotel.feedBacks.Where(x => !x.Status.Equals("Hidden")).Average(feedback => feedback.Rating), 1) : 0,
                              });
            return new ResponseMessage { Success = true, Data = listHotel, Message = "Succsessfully", StatusCode = (int)HttpStatusCode.OK };

        }

        public ResponseMessage BlockedHotel(int hotelID, String reaseonBlock)
        {
            var getHotel = db.hotel
                             .Include(account => account.Account)
                             .FirstOrDefault(hotel => hotel.HotelID == hotelID);
            if (getHotel == null)
            {
                return new ResponseMessage { Success = true, Data = getHotel, Message = "Data not found", StatusCode = (int)HttpStatusCode.OK };
            }
            var getAccount = db.accounts.FirstOrDefault(account => account.AccountID == getHotel.Account.AccountID);
            if (getAccount == null)
            {
                return new ResponseMessage { Success = true, Data = getHotel, Message = "Data not found", StatusCode = (int)HttpStatusCode.OK };
            }

            getHotel.isRegister = "Blocked";
            getHotel.Status = false;
            getAccount.IsActive = false;
            db.hotel.Update(getHotel);
            db.accounts.Update(getAccount);
            db.SaveChanges();
            Ultils.Utils.SendMailRegistration(getAccount.Email, reaseonBlock);
            return new ResponseMessage { Success = true, Data = getHotel, StatusCode = (int)HttpStatusCode.OK };


        }

        public ResponseMessage ConfirmRegistration(int hotelID)
        {
            var getHotel = db.hotel
                             .Include(account => account.Account)
                             .Include(hotelService => hotelService.HotelServices)
                             .ThenInclude(hotelSubService => hotelSubService.HotelSubServices)
                             .Include(address => address.HotelAddress)
                             .FirstOrDefault(hotel => hotel.HotelID == hotelID);

            if (getHotel != null)
            {
                var getAccount = db.accounts.FirstOrDefault(account => account.AccountID == getHotel.Account.AccountID);
                getHotel.isRegister = "Approved";
                getHotel.Status = true;
                getAccount.IsActive = true;
                db.hotel.Update(getHotel);
                db.SaveChanges();
                String mailContent = "You have been approved to become our partner. Please log in to the system and perform activities.";
                Ultils.Utils.SendMailRegistration(getAccount.Email, mailContent);
                return new ResponseMessage { Success = true, Data = getHotel, Message = "Sucessfully", StatusCode = (int)HttpStatusCode.OK };
            }
            return new ResponseMessage { Success = false, Data = getHotel, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };
        }

        public ResponseMessage FilterHotelByStatus(bool Status)
        {
            var filterHotel = db.hotel.Where(hotel => hotel.Status == Status).ToList();
            return new ResponseMessage { Success = true, Data = filterHotel, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
        }

        public ResponseMessage RejectRegistration(int hotelID, String reasonReject)
        {
            var getHotel = db.hotel
                             .Include(account => account.Account)
                             .FirstOrDefault(hotel => hotel.HotelID == hotelID);
            if (getHotel == null)
            {
                return new ResponseMessage { Success = false, Data = getHotel, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };

            }
            var getAccount = db.accounts
                               .Include(role => role.Role)
                               .FirstOrDefault(account => account.AccountID == getHotel.Account.AccountID);
            if (getAccount == null)
            {
                return new ResponseMessage { Success = false, Data = getHotel, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };

            }
            String Role = "Customer";
            var getRole = db.roles.FirstOrDefault(role => role.Name.Equals(Role));
            getHotel.isRegister = "Rejected";
            getAccount.Role = getRole;
            db.hotel.Update(getHotel);
            db.accounts.Update(getAccount);
            db.SaveChanges();
            Ultils.Utils.SendMailRegistration(getAccount.Email, reasonReject);
            return new ResponseMessage { Success = true, Data = getHotel, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };


        }

        public ResponseMessage SearchHotelByName(string hotelName)
        {
            var searchResult = db.hotel
                              .Include(feedbacks => feedbacks.feedBacks)
                              .Include(hotelService => hotelService.HotelServices)
                              .ThenInclude(hotelSubService => hotelSubService.HotelSubServices)
                              .Include(address => address.HotelAddress)
                              .Include(account => account.Account)
                              .ThenInclude(profile => profile.Profile)
                              .Where(hotel => hotel.Name.Contains(hotelName))
                              .ToList()
                              .Select(hotel => new
                              {
                                  HotelID = hotel.HotelID,
                                  MainImage = hotel.MainImage,
                                  Name = hotel.Name,
                                  OpenedIn = hotel.OpenedIn,
                                  Description = hotel.Description,
                                  HotelStandar = hotel.HotelStandar,
                                  IsRegister = hotel.isRegister,
                                  Status = hotel.Status,
                                  Account = new
                                  {
                                      AccountID = hotel.Account.AccountID,
                                      Email = hotel.Account.Email,
                                      Password = hotel.Account.Password,
                                      Phone = hotel.Account.Phone,
                                      IsActive = hotel.Account.IsActive,
                                      Profile = new
                                      {
                                          ProfileID = hotel.Account.Profile.ProfileID,
                                          FullName = hotel.Account.Profile.fullName,
                                          BirthDay = hotel.Account.Profile.BirthDay,
                                          Gender = hotel.Account.Profile.Gender,
                                          Avatar = hotel.Account.Profile.Avatar
                                      }
                                  },
                                  HotelAddress = hotel.HotelAddress,
                                  HotelService = hotel.HotelServices.Select(service => new
                                  {
                                      ServiceID = service.ServiceID,
                                      Type = service.Type,
                                      HotelSubService = service.HotelSubServices.Select(sub => new
                                      {
                                          SubServiceID = sub.SubServiceID,
                                          SubServiceName = sub.SubServiceName
                                      })

                                  }),

                                  FeedBack = hotel.feedBacks.Where(x => !x.Status.Equals("Hidden")).Select(feedbacks => new
                                  {
                                      FeedbackID = feedbacks.FeedBackID,
                                      Rating = feedbacks.Rating,
                                      Image = feedbacks.Image,
                                      Description = feedbacks.Description,
                                      Status = feedbacks.Status
                                  }),
                                  TotalBooking = db.booking.Count(booking => booking.Room.Hotel.HotelID == hotel.HotelID),
                                  TotalRevenue = db.booking.Where(booking => booking.Room.Hotel.HotelID == hotel.HotelID).Sum(booking => booking.TotalPrice),
                                  AvgRating = hotel.feedBacks.Where(x => !x.Status.Equals("Hidden")).Any()
                                               ? Math.Round(hotel.feedBacks.Where(x => !x.Status.Equals("Hidden")).Average(feedback => feedback.Rating), 1) : 0,
                              });
            return new ResponseMessage { Success = true, Data = searchResult, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
        }

        public ResponseMessage GetAllHotelWaitForApproval()
        {
            var listHotel = db.hotel
                              .Include(hotelService => hotelService.HotelServices)
                              .ThenInclude(hotelSubService => hotelSubService.HotelSubServices)
                              .Include(address => address.HotelAddress)
                              .Include(account => account.Account)
                              .Include(profile => profile.Account.Profile)
                              .Where(hotel => hotel.isRegister.Equals("Awaiting Approval") && hotel.Status == false)
                              .ToList();

            var responseData = listHotel.Select(hotel => new
            {
                HotelID = hotel.HotelID,
                MainImage = hotel.MainImage,
                Name = hotel.Name,
                OpenedIn = hotel.OpenedIn,
                Description = hotel.Description,
                HotelStandar = hotel.HotelStandar,
                IsRegister = hotel.isRegister,
                Status = hotel.Status,
                Account = new
                {
                    AccountID = hotel.Account.AccountID,
                    Email = hotel.Account.Email,
                    Phone = hotel.Account.Phone,
                    IsActive = hotel.Account.IsActive,
                    Profile = new
                    {
                        ProfileID = hotel.Account.Profile.ProfileID,
                        FullName = hotel.Account.Profile.fullName,
                        BirthDay = hotel.Account.Profile.BirthDay,
                        Gender = hotel.Account.Profile.Gender,
                        Address = hotel.Account.Profile.Address,
                        Avatar = hotel.Account.Profile.Avatar
                    }
                },
                HotelAddress = new
                {
                    AddressID = hotel.HotelAddress.AddressID,
                    Address = hotel.HotelAddress.Address,
                    City = hotel.HotelAddress.City,
                    Latitude = hotel.HotelAddress.latitude,
                    Longitude = hotel.HotelAddress.longitude
                },
                HotelServices = hotel.HotelServices.Select(service => new
                {
                    ServiceID = service.ServiceID,
                    Type = service.Type,
                    HotelSubServices = service.HotelSubServices.Select(subService => new
                    {
                        SubServiceID = subService.SubServiceID,
                        SubServiceName = subService.SubServiceName
                    }).ToList()
                }).ToList()
            }).ToList();

            return new ResponseMessage { Success = true, Data = responseData, Message = "Sucessfully", StatusCode = (int)HttpStatusCode.OK };
        }

        public ResponseMessage AnalyzeHotelStandar()
        {
            var listHotel = db.hotel.ToList();
            var hotelStandar = new Dictionary<string, int>();
            for (int star = 1; star <= 5; star++)
            {
                hotelStandar[$"{star} Star"] = 0;
            }

            foreach (var hotel in listHotel)
            {
                if (hotelStandar.ContainsKey($"{hotel.HotelStandar} Star"))
                {
                    hotelStandar[$"{hotel.HotelStandar} Star"]++;
                }
            }

            var hotelStarCounts = hotelStandar.Select(h => new HotelStandarDTO
            {
                Name = $"Hotel {h.Key}",
                Value = h.Value
            }).ToList();

            return new ResponseMessage { Success = true, Data = hotelStarCounts, Message = "Sucessfully", StatusCode = (int)HttpStatusCode.OK };

        }

        public ResponseMessage GetGuestReviewByHotel(int hotelID)
        {
            var listReview = db.feedback
                               .Include(hotel => hotel.Hotel)
                               .Include(booking => booking.Booking)
                               .Include(account => account.Account)
                               .ThenInclude(profile => profile.Profile)
                               .Include(room => room.Booking.Room)
                               .Where(fb => fb.Hotel.HotelID == hotelID && !fb.Status.Equals("Hidden"))
                               .ToList()
                               .Select(feedback => new
                               {
                                   FeedbackID = feedback.FeedBackID,
                                   Rating = feedback.Rating,
                                   Image = feedback.Image,
                                   Description = feedback.Description,
                                   Booking = new
                                   {
                                       CheckInDate = feedback.Booking.CheckInDate
                                   },
                                   Room = new
                                   {
                                       TypeOfRoom = feedback.Booking.Room.TypeOfRoom
                                   },
                                   Profile = new
                                   {
                                       Avatar = feedback.Account.Profile.Avatar,
                                       FullName = feedback.Account.Profile.fullName
                                   }
                               }).ToList();
            var AvgRating = listReview.Any() ? Math.Round(listReview.Average(rt => rt.Rating), 1) : 0;
            var CountFeedback = listReview.Count();
            return new ResponseMessage
            {
                Success = true,
                Data = new { listReview = listReview, AvgRating = AvgRating, CountFeedback = CountFeedback },
                Message = "Successfully",
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        public ResponseMessage GetBasicInformation(int hotelID)
        {
            var basicInformation = db.hotel
                                     .Include(x => x.HotelAddress)
                                     .FirstOrDefault(hotel => hotel.HotelID == hotelID);
            if (basicInformation != null)
            {
                return new ResponseMessage { Success = true, Data = basicInformation, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
            return new ResponseMessage { Success = false, Data = basicInformation, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };
        }

        public ResponseMessage GetAddressByHotel(int hotelID)
        {
            var address = db.hotel
                            .Include(address => address.HotelAddress)
                            .FirstOrDefault(x => x.HotelID == hotelID);
            if (address != null)
            {
                var result = new
                {
                    AddressID = address.HotelAddress.AddressID,
                    Address = address.HotelAddress.Address,
                    City = address.HotelAddress.City,
                    Latidue = address.HotelAddress.latitude,
                    Lontidue = address.HotelAddress.longitude
                };
                return new ResponseMessage { Success = true, Message = "Successfully", Data = result, StatusCode = (int)HttpStatusCode.OK };
            }
            return new ResponseMessage { Success = false, Message = "Data not found", Data = address, StatusCode = (int)HttpStatusCode.NotFound };
        }

        public ResponseMessage UpdateAddressByHotel(int hotelID,HotelAddress newAddress)
        {
            var address = db.hotel.Include(address => address.HotelAddress)
                                  .FirstOrDefault(x => x.HotelID == hotelID);
            if (address != null)
            {
                address.HotelAddress.Address = Ultils.Utils.RemoveVietnameseDiacritics(newAddress.Address);
                address.HotelAddress.City = Ultils.Utils.RemoveVietnameseDiacritics(newAddress.City);
                address.HotelAddress.latitude = newAddress.latitude;
                address.HotelAddress.longitude = newAddress.longitude;
                db.hotel.Update(address);
                db.SaveChanges();
                return new ResponseMessage { Success = true, Message = "Successfully", Data = address, StatusCode = (int)HttpStatusCode.OK };

            }
            return new ResponseMessage { Success = false, Message = "Data not found", Data = address, StatusCode = (int)HttpStatusCode.NotFound };

        }
        public ResponseMessage SearchHotel(string name)
        {
            var currentDate = DateTime.Now.AddHours(14);
            var listHotel = db.hotel
                              .Include(address => address.HotelAddress)
                              .Include(hotelService => hotelService.HotelServices)
                              .ThenInclude(subService => subService.HotelSubServices)
                              .Include(feedback => feedback.feedBacks)
                              .Include(room => room.rooms)
                              .ThenInclude(specialPrice => specialPrice.SpecialPrice)
                              .Where(hotel => hotel.Name.Contains(name) && hotel.isRegister.Equals("Approved") && hotel.Status == true)
                              .ToList();

            var responseData = listHotel.Select(hotel => new
            {
                HotelID = hotel.HotelID,
                Name = hotel.Name,
                MainImage = hotel.MainImage,
                OpenedIn = hotel.OpenedIn,
                Description = hotel.Description,
                HotelStandar = hotel.HotelStandar,
                IsRegister = hotel.isRegister,
                Status = hotel.Status,
                HotelAddress = new
                {
                    hotel.HotelAddress.AddressID,
                    hotel.HotelAddress.Address,
                    hotel.HotelAddress.City,
                    hotel.HotelAddress.latitude,
                    hotel.HotelAddress.longitude
                },
                HotelService = hotel.HotelServices.Select(service => new
                {
                    ServiceID = service.ServiceID,
                    ServiceType = service.Type,
                    HotelSubService = service.HotelSubServices.Select(subService => new
                    {
                        SubServiceID = subService.SubServiceID,
                        Name = subService.SubServiceName
                    }).ToList()
                }).ToList(),

                Rooms = hotel.rooms
                        .Where(room => room != null && room.Quantity > 0) // Lọc giá trị null trong rooms
                        .Select(room =>
                        {
                            var currentDate = DateTime.Now.AddHours(14);
                            var specialPrice = room.SpecialPrice
                                .FirstOrDefault(sp => currentDate.Date >= sp.StartDate && currentDate.Date <= sp.EndDate);
                            if (specialPrice != null)
                            {
                                room.Price = specialPrice.Price;
                            }
                            return new
                            {
                                RoomID = room.RoomID,
                                TypeOfRoom = room.TypeOfRoom,
                                NumberCapacity = room.NumberCapacity,
                                Price = room.Price,
                                Quantity = room.Quantity,
                                SizeOfRoom = room.SizeOfRoom,
                                TypeOfBed = room.TypeOfBed,
                                SpecialPrice = room.SpecialPrice.Select(sp => new
                                {
                                    SpecialPriceID = sp.SpecialPriceID,
                                    StartDate = sp.StartDate,
                                    EndDate = sp.EndDate,
                                    Price = sp.Price
                                }).ToList(),
                            };
                        }).OrderBy(x => x.Price).ToList(),
                FeedBack = hotel.feedBacks.Select(feedback => new
                {
                    FeedBackID = feedback.FeedBackID,
                    Rating = feedback.Rating,
                    Image = feedback.Image,
                    Description = feedback.Description,
                    Status = feedback.Status,
                    Account = feedback.Account
                }).Where(x => !x.Status.Equals("Hidden")).ToList(),
                AvgRating = hotel.feedBacks.Where(x => !x.Status.Equals("Hidden")).Any()
                                ? Math.Round(hotel.feedBacks.Where(x => !x.Status.Equals("Hidden")).Average(feedback => feedback.Rating), 1) : 0,
            }).ToList();


            return new ResponseMessage { Success = true, Message = "Successfully", Data = responseData, StatusCode = (int)HttpStatusCode.OK };
        }


        public ResponseMessage GetAllCity()
        {
            var listAddress = db.hotelAddress
                                .Select(address => address.City.Trim())
                                .Distinct()
                                .ToList()
                                .Select(city => new
                                {
                                    City = city
                                });

            return new ResponseMessage { Success = true, Message = "Successfully", Data = listAddress, StatusCode = (int)HttpStatusCode.OK };
        }

        public ResponseMessage SearchHotelCity(String city)
        {
            var currentDate = DateTime.Now.AddHours(14);
            var listHotel = db.hotel
                              .Include(address => address.HotelAddress)
                              .Include(hotelService => hotelService.HotelServices)
                              .ThenInclude(subService => subService.HotelSubServices)   
                              .Include(feedback => feedback.feedBacks)
                              .Include(room => room.rooms)
                              .ThenInclude(specialPrice => specialPrice.SpecialPrice)
                              .Where(hotel => hotel.HotelAddress.City.Equals(city) && hotel.isRegister.Equals("Approved") && hotel.Status == true)
                              .ToList();
            if (listHotel.Any())
            {
                var listHotelWithAvgRating = listHotel.Select(hotel => new
                {
                    HotelID = hotel.HotelID,
                    Name = hotel.Name,
                    MainImage = hotel.MainImage,
                    OpenedIn = hotel.OpenedIn,
                    Description = hotel.Description,
                    HotelStandar = hotel.HotelStandar,
                    IsRegister = hotel.isRegister,
                    Status = hotel.Status,
                    HotelAddress = new
                    {
                        hotel.HotelAddress.AddressID,
                        hotel.HotelAddress.Address,
                        hotel.HotelAddress.City,
                        hotel.HotelAddress.latitude,
                        hotel.HotelAddress.longitude
                    },
                    HotelService = hotel.HotelServices.Select(service => new
                    {
                        ServiceID = service.ServiceID,
                        ServiceType = service.Type,
                        HotelSubService = service.HotelSubServices.Select(subService => new
                        {
                            SubServiceID = subService.SubServiceID,
                            Name = subService.SubServiceName
                        }).ToList()
                    }).ToList(),

                    Rooms = hotel.rooms
                                       .Where(room => room != null && room.Quantity > 0) // Lọc giá trị null trong rooms
                                       .Select(room =>
                                       {
                                           var currentDate = DateTime.Now.AddHours(14);
                                           var specialPrice = room.SpecialPrice
                                               .FirstOrDefault(sp => currentDate.Date >= sp.StartDate && currentDate.Date <= sp.EndDate);
                                           if (specialPrice != null)
                                           {
                                               room.Price = specialPrice.Price;
                                           }
                                           return new
                                           {
                                               RoomID = room.RoomID,
                                               TypeOfRoom = room.TypeOfRoom,
                                               NumberCapacity = room.NumberCapacity,
                                               Price = room.Price,
                                               Quantity = room.Quantity,
                                               SizeOfRoom = room.SizeOfRoom,
                                               TypeOfBed = room.TypeOfBed,
                                               SpecialPrice = room.SpecialPrice.Select(sp => new
                                               {
                                                   SpecialPriceID = sp.SpecialPriceID,
                                                   StartDate = sp.StartDate,
                                                   EndDate = sp.EndDate,
                                                   Price = sp.Price
                                               }).ToList(),
                                           };
                                       }).OrderBy(x => x.Price).ToList(),
                    FeedBack = hotel.feedBacks.Where(x => !x.Status.Equals("Hidden")).Select(feedback => new
                    {
                        FeedBackID = feedback.FeedBackID,
                        Rating = feedback.Rating,
                        Image = feedback.Image,
                        Description = feedback.Description,
                        Status = feedback.Status,
                        Account = feedback.Account
                    }).ToList(),
                    AvgRating = hotel.feedBacks.Any() ? Math.Round(hotel.feedBacks.Where(x => !x.Status.Equals("Hidden")).Average(feedback => feedback.Rating), 1) : 0,
                }).ToList();
                return new ResponseMessage { Success = true, Message = "Successfully", Data = listHotelWithAvgRating, StatusCode = (int)HttpStatusCode.OK };
            }
            else
            {
                return new ResponseMessage { Success = false, Message = "Data not found", Data = listHotel, StatusCode = (int)HttpStatusCode.NotFound };

            }
        }
    }
}



