using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraduationAPI_EPOSHBOOKING.DTO
{
    public class TelegramUpdate
    {
        public long UpdateId { get; set; }
        public TelegramMessage Message { get; set; }
    }
}
    