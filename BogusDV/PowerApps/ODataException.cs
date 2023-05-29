using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BogusDV.PowerApps
{
    public class ODataException
    {
        public string? Message { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? StackTrace { get; set; }
        public string? ErrorCode { get; set; }
    }
}
