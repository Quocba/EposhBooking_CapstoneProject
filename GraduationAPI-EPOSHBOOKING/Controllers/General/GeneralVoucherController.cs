using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using GraduationAPI_EPOSHBOOKING.Ultils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Controllers.Guest
{
    [Route("api/v1/voucher")]
    [ApiController]
    public class GeneralVoucherController : ControllerBase
    {
        private readonly IVoucherRepository _voucherRepository;

        public GeneralVoucherController(IVoucherRepository voucherRepository)
        {

            this._voucherRepository = voucherRepository;
        }

        [HttpGet("get-all-voucher")]
        public IActionResult GetAllVouchers()
        {

            var response = _voucherRepository.GetAllVouchers();
            return StatusCode(response.StatusCode, response);

        }
        [HttpGet("get-voucher-id")]
        public IActionResult GetVoucherById(int voucherId)
        {
            var response = _voucherRepository.GetVoucherById(voucherId);
            return StatusCode(response.StatusCode, response);
        }


    }
}
