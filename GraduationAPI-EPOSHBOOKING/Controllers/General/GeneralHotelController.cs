
using GraduationAPI_EPOSHBOOKING.IRepository;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Repository;
using Microsoft.AspNetCore.Mvc;

using System.Net;
using GraduationAPI_EPOSHBOOKING.DTO;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Controllers.Guest
{
    [ApiController]
    [Route("api/v1/hotel")]
    public class GeneralHotelController : Controller
    {
        private readonly IHotelRepository repository;
        public GeneralHotelController(IHotelRepository hotelRepository)
        {
            this.repository = hotelRepository;
        }
        //[HttpGet("filter-status")]
        //public IActionResult FilterHotelByStatus([FromQuery]bool status)
        //{
        //    var response= repository.FilterHotelByStatus(status);
        //    return StatusCode(response.StatusCode, response);
        //}
        [HttpGet("get-all")]
        public IActionResult GetAllHotel()
        {
            var respone = repository.GetAllHotel();
            return StatusCode(respone.StatusCode, respone);
        }
        [HttpGet("get-by-city")]
        public IActionResult GetHotelByCity([FromQuery] String city)
        {

            var reponse = repository.GetHotelByCity(city);
            return StatusCode(reponse.StatusCode, reponse);

        }
        [HttpGet("get-by-id")]
        public IActionResult GetHotelByID([FromQuery] int id)
        {
            var response = repository.GetHotelByID(id); 
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-by-price")]
        public IActionResult getHotelByPrice([FromForm] String city, [FromForm] double minPrice, [FromForm] double maxPrice)
        {
            var response = repository.GetHotelByPrice(city, minPrice, maxPrice);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-by-standar")]
        public IActionResult GetByHotelStandar([FromQuery] int hotelStandar)
        {
            var response = repository.GetByHotelStandar(hotelStandar);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-by-service")]
        public IActionResult GetHotelByService([FromForm] List<string> service)
        {
            var response = repository.GetByService(service);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-service-by-hotelID")]
        public IActionResult GetServiceByHotel([FromQuery] int hotelID)
        {
            var response = repository.GetServiceByHotelID(hotelID);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-guest-review")]
        public IActionResult GetGuestReviewByHotel([FromQuery] int hotelID)
        {
            var response = repository.GetGuestReviewByHotel(hotelID);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("search")]
        public IActionResult SearchMobile([FromQuery]String name)
        {
            var response = repository.SearchHotel(name);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-city")]
        public IActionResult GetAllCity()
        {
            var response = repository.GetAllCity();
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("search-hotel-city")]
        public IActionResult SearchByCity([FromForm]String city)
        {
            var response = repository.SearchHotelCity(city);
            return StatusCode(response.StatusCode, response);
        }

    }
}
 