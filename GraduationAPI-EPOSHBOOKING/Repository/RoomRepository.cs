using GraduationAPI_EPOSHBOOKING.DataAccess;
using GraduationAPI_EPOSHBOOKING.DTO;
using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System.Net;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Repository
{
    public class RoomRepository : IRoomRepository
    {
        private readonly DBContext db;
        private readonly IWebHostEnvironment environment;
        public RoomRepository(DBContext _db, IWebHostEnvironment environment)
        {
            this.db = _db;
            this.environment = environment;
        }

        public ResponseMessage GetRoomDetail(int roomID)
        {
            var getRoom = db.room.Include(x => x.RoomImages).Include(x => x.SpecialPrice).Include(x => x.RoomService).ThenInclude(x => x.RoomSubServices)
            .FirstOrDefault(room => room.RoomID == roomID);
           
            if (getRoom != null)
            {
               return new ResponseMessage { Success = true, Data = getRoom, Message = "Successfully",StatusCode = (int)HttpStatusCode.OK };
            }
                return new ResponseMessage { Success = false,Data = getRoom, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };
        }
        //Không dùng tới
        public ResponseMessage GetAllRoom()
        {
            var listRoom = db.room.Include(service => service.RoomService)
                .ThenInclude(subService => subService.RoomSubServices)
                .Include(specialPrice => specialPrice.SpecialPrice)
                .Include(img => img.RoomImages).ToList();
            if (listRoom.Any())
            {
                return new ResponseMessage { Success = true, Data = listRoom, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
            return new ResponseMessage { Success = false, Data = listRoom, Message = "No Data", StatusCode = (int)HttpStatusCode.NotFound };
        }

        public ResponseMessage DeleteRoom(int roomID)
        {
            var getRoom = db.room.FirstOrDefault(room => room.RoomID == roomID);
            if (getRoom != null)
            {
                //var bookings = db.booking.Include(Room => Room.Room).Where(b => b.Room.RoomID == roomID).ToList();

                //if (bookings.Any())
                //{
                //    foreach (var booking in bookings)
                //    {
                //        db.booking.Remove(booking);
                //    }
                //    db.SaveChanges();
                //}
                getRoom.Quantity = 0;
                db.room.Update(getRoom);
                db.SaveChanges();
                return new ResponseMessage { Success = true, Data = getRoom, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
                return new ResponseMessage { Success = false,Data = getRoom, Message = "Data not found",StatusCode = (int)HttpStatusCode.NotFound}; 
        }

        public ResponseMessage AddRoom(int hotelID, Room room,List<SpecialPrice>specialPrices, List<IFormFile> images,List<ServiceTypeDTO>services)
        {
            var getHotel = db.hotel.FirstOrDefault(hotel => hotel.HotelID == hotelID);
            Room createRoom = new Room
            {
                TypeOfBed = room.TypeOfBed,
                NumberCapacity = room.NumberCapacity,
                Price = room.Price,
                Quantity = room.Quantity,
                SizeOfRoom = room.SizeOfRoom,
                TypeOfRoom = room.TypeOfRoom,
                NumberOfBed = room.NumberOfBed,
                Hotel = getHotel
            };
            db.room.Add(createRoom);

            foreach (var specialPrice in specialPrices)
            {
              
                SpecialPrice addSpecialPrice = new SpecialPrice
                {
                    StartDate = specialPrice.StartDate,
                    EndDate = specialPrice.EndDate,
                    Price = specialPrice.Price,
                    Room = createRoom
                };
                db.specialPrice.Add(addSpecialPrice);
            }
             

            foreach (var image in images)
            {
                
                RoomImage addImage = new RoomImage
                {
                    Image = Ultils.Utils.SaveImage(image,environment),
                    Room = createRoom
                };
                HotelImage addHotelImage = new HotelImage
                {
                    Image = Ultils.Utils.SaveImage(image,environment),
                    Title = "Rooms",
                    Hotel = getHotel
                };
                db.roomImage.Add(addImage);
                db.hotelImage.Add(addHotelImage);
            }

            foreach (var service in services)
            {
                var addService = new RoomService
                {
                    Type = service.serviceType,
                    Room = createRoom
                };
                db.roomService.Add(addService);
                var roomSubService = new List<RoomSubService>();
                foreach (var subServiceName in service.subServiceName)
                {
                    var addSubService = new RoomSubService
                    {
                        SubName = subServiceName,
                        RoomService = addService
                    };
                    db.roomSubService.Add(addSubService);
                    roomSubService.Add(addSubService);
                }
                addService.RoomSubServices = roomSubService;

            }
            var checkRoom = db.room.Include(hotel => hotel.Hotel)
                       .Where(room => room.Hotel.HotelID == hotelID).ToList();
            if (!checkRoom.Any())
            {
                //var totalQuantity = db.room.Where(hotel => hotel.Hotel.HotelID == getHotel.HotelID)
                //.Sum(room => room.Quantity);
                if (room.Quantity > 0 && room.Quantity <= 10)
                {
                    getHotel.HotelStandar = 1;
                    db.hotel.Update(getHotel);
                    db.SaveChanges();
                }
                if (room.Quantity >= 20 && room.Quantity <= 49)
                {
                    getHotel.HotelStandar = 2;
                    db.hotel.Update(getHotel);
                    db.SaveChanges();
                }
                if (room.Quantity >= 50 && room.Quantity <= 79)
                {
                    getHotel.HotelStandar = 3;
                    db.hotel.Update(getHotel);
                    db.SaveChanges();
                }
                if (room.Quantity >= 80 && room.Quantity <= 99)
                {
                    getHotel.HotelStandar = 4;
                    db.hotel.Update(getHotel);
                    db.SaveChanges();
                }
                if (room.Quantity >= 100)
                {
                    getHotel.HotelStandar = 5;
                    db.hotel.Update(getHotel);
                    db.SaveChanges();
                }   
            }

            if (checkRoom.Any())
            {
                var totalQuantity = db.room.Where(hotel => hotel.Hotel.HotelID == getHotel.HotelID)
               .Sum(room => room.Quantity);
                if (totalQuantity > 0 && totalQuantity <= 10)
                {
                    getHotel.HotelStandar = 1;
                    db.hotel.Update(getHotel);
                    db.SaveChanges();
                }
                if (totalQuantity >= 20 && totalQuantity <= 49)
                {
                    getHotel.HotelStandar = 2;
                    db.hotel.Update(getHotel);
                    db.SaveChanges();
                }
                if (totalQuantity >= 50 && totalQuantity <= 79)
                {
                    getHotel.HotelStandar = 3;
                    db.hotel.Update(getHotel);
                    db.SaveChanges();
                }
                if (totalQuantity >= 80 && totalQuantity <= 99)
                {
                    getHotel.HotelStandar = 4;
                    db.hotel.Update(getHotel);
                    db.SaveChanges();
                }
                if (totalQuantity >= 100)
                {
                    getHotel.HotelStandar = 5;
                    db.hotel.Update(getHotel);
                    db.SaveChanges();
                }
            }
            
                db.SaveChanges();
                return new ResponseMessage { Success = true, Data = createRoom, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };     
        }

        public ResponseMessage UpdateRoom(int roomID, Room room, List<SpecialPrice>SpecialPrices,String urlImage, List<IFormFile> image, 
            List<ServiceTypeDTO>services)
        {
            var getRoom = db.room
                  .Include(room => room.Hotel)
                  .Include(room => room.SpecialPrice)
                  .Include(room => room.RoomService)
                  .ThenInclude(roomService => roomService.RoomSubServices)
                  .FirstOrDefault(room => room.RoomID == roomID);
            if (getRoom == null)
            {
                return new ResponseMessage { Success = false, Data = getRoom, Message = "Data not found", StatusCode = (int)HttpStatusCode.OK };
            }
            else
            {
                 getRoom.TypeOfRoom = room.TypeOfRoom;
                 getRoom.NumberCapacity = room.NumberCapacity;
                 getRoom.Price = room.Price;
                 getRoom.Quantity = room.Quantity;
                 getRoom.SizeOfRoom = room.SizeOfRoom;
                 getRoom.TypeOfBed = room.TypeOfBed;
                 getRoom.NumberOfBed = room.NumberOfBed;
                 db.room.Update(getRoom);

                var getSpecialPriceRoom = db.specialPrice.Where(sp => sp.Room.RoomID == roomID).ToList();
                if (getSpecialPriceRoom.Any())
                {
                    db.specialPrice.RemoveRange(getSpecialPriceRoom);
                }
                if (getSpecialPriceRoom.Any())
                {
                    db.specialPrice.RemoveRange(getSpecialPriceRoom);
                    foreach (var specialPrice in SpecialPrices)
                    {
                        SpecialPrice addSpecialprice = new SpecialPrice
                        {
                            StartDate = specialPrice.StartDate,
                            EndDate = specialPrice.EndDate,
                            Price = specialPrice.Price,
                            Room = getRoom
                        };
                        db.specialPrice.Add(addSpecialprice);
                    }
                    db.SaveChanges();
                }
                else
                {
               
                    foreach (var specialPrice in SpecialPrices)
                    {
                       
                       
                        SpecialPrice addSpecialprice = new SpecialPrice
                        {

                            StartDate = specialPrice.StartDate,
                            EndDate = specialPrice.EndDate,
                            Price = specialPrice.Price,
                            Room = getRoom
                        };
                         db.specialPrice.Add(addSpecialprice);
                        
                    }
                         db.SaveChanges();
                }
                var existingImages = db.roomImage.Where(ri => ri.Room.RoomID == roomID).ToList();
                db.roomImage.RemoveRange(existingImages);
                db.SaveChanges();
                if (urlImage != null)
                {
                    var urlImages = JsonConvert.DeserializeObject<List<string>>(urlImage);
                    foreach (var imageUrl in urlImages)
                    {
                        RoomImage oldImage = new RoomImage
                        {
                            Image = imageUrl,
                            Room = getRoom
                        };
                        HotelImage oldHotelImage = new HotelImage
                        {
                            Image = imageUrl,
                            Title = "Room",
                            Hotel = getRoom.Hotel
                        };
                        db.roomImage.Add(oldImage);
                        db.hotelImage.Add(oldHotelImage);
                    }
                }   
                if (image != null && image.Any())
                {
                    foreach(var newImage in image)
                    {
                        RoomImage addNewImage = new RoomImage
                        {
                            Image = Ultils.Utils.SaveImage(newImage, environment),
                            Room = getRoom
                        };
                        HotelImage addNewHotelImage = new HotelImage
                        {
                            Image = Ultils.Utils.SaveImage(newImage, environment),
                            Title = "Rooms",
                            Hotel = getRoom.Hotel
                        };
                        db.roomImage.Add(addNewImage);
                        db.hotelImage.Add(addNewHotelImage);
                     
                    }
                
                }

                var existingServices = db.roomService.Where(rs => rs.Room.RoomID == roomID).ToList();
                db.roomService.RemoveRange(existingServices);

                foreach (var service in services)
                {
                    var addService = new RoomService
                    {
                        Type = service.serviceType,
                        Room = getRoom
                    };
                    db.roomService.Add(addService);
                    db.SaveChanges();
                    var roomSubService = new List<RoomSubService>();
                    foreach (var subServiceName in service.subServiceName)
                    {   
                        var addSubService = new RoomSubService
                        {
                            SubName = subServiceName,
                            RoomService = addService
                        };
                        db.roomSubService.Add(addSubService);
                        roomSubService.Add(addSubService);
                    }
                    addService.RoomSubServices = roomSubService;

                }


                var HotelID = getRoom.Hotel.HotelID;
                var totalQuantity = db.room.Where(r => r.Hotel.HotelID == HotelID).Sum(r => r.Quantity);
                var getHotel = db.hotel.FirstOrDefault(hotel => hotel.HotelID == HotelID);
                var checkRoom = db.room
                                  .Include(x => x.Hotel)
                                  .FirstOrDefault(x => x.Hotel.HotelID == HotelID);
                if(checkRoom == null)
                {
                    if (totalQuantity > 0 && totalQuantity <= 10)
                    {
                        getHotel.HotelStandar = 1;
                        db.hotel.Update(getHotel);
                        db.SaveChanges();
                    }
                    if (totalQuantity >= 20 && totalQuantity <= 49)
                    {
                        getHotel.HotelStandar = 2;
                        db.hotel.Update(getHotel);
                        db.SaveChanges();
                    }
                    if (totalQuantity >= 50 && totalQuantity <= 79)
                    {
                        getHotel.HotelStandar = 3;
                        db.hotel.Update(getHotel);
                        db.SaveChanges();
                    }
                    if (totalQuantity >= 80 && totalQuantity <= 99)
                    {
                        getHotel.HotelStandar = 4;
                        db.hotel.Update(getHotel);
                        db.SaveChanges();
                    }
                    if (totalQuantity >= 100)
                    {
                        getHotel.HotelStandar = 5;
                        db.hotel.Update(getHotel);
                        db.SaveChanges();
                    }
                    return new ResponseMessage { Success = true, Data = getRoom, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
                }
                else
                {
                    return new ResponseMessage { Success = true, Data = getRoom, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
                }
 
            }
        }

        public ResponseMessage GetRoomByHotel(int hotelID)
        {
            var currentDate = DateTime.Now.AddHours(14);
            var listRoomWithHotel = db.room
                                      .Include(hotel => hotel.Hotel)
                                      .Include(roomService => roomService.RoomService)
                                      .ThenInclude(roomSbuService => roomSbuService.RoomSubServices)
                                      .Include(roomImage => roomImage.RoomImages)
                                      .Include(specialPrice => specialPrice.SpecialPrice)
                                      .Where(x => x.Hotel.HotelID == hotelID)
                                      .ToList();
            var updatedRoomList = listRoomWithHotel.Select(room =>
            {
                return new
                {
                    RoomID = room.RoomID,
                    TypeOfRoom = room.TypeOfRoom,
                    NumberCapacity = room.NumberCapacity,
                    Price = room.Price,
                    Quantity = room.Quantity,
                    SizeOfRoom = room.SizeOfRoom,
                    TypeOfBed = room.TypeOfBed,
                    NumberOfBed = room.NumberOfBed,
                    RoomServices = room.RoomService.Select(rs => new
                    {
                        rs.RoomServiceID,
                        rs.Type,
                        RoomSubServices = rs.RoomSubServices.Select(rss => new
                        {
                            rss.SubServiceID,
                            rss.SubName
                        }).ToList()
                    }).ToList(),
                    RoomImages = room.RoomImages.Select(img => new
                    {
                        img.ImageID,
                        img.Image
                    }).ToList(),
                    SpecialPrice = room.SpecialPrice.Select(sp => new
                    {
                        sp.SpecialPriceID,
                        sp.StartDate,
                        sp.EndDate,
                        sp.Price
                    }).ToList()
                };
            }).ToList();


            return new ResponseMessage { Success = true, Data = updatedRoomList, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
        }
    }
}
