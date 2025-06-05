using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraduationAPI_EPOSHBOOKING.DTO
{
    public class TelegramResponse
    {

            public bool Ok { get; set; }
            public List<TelegramUpdate> Result { get; set; }
        
    }
}
