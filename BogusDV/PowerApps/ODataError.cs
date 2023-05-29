using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BogusDV.PowerApps
{
    public class ODataError
    {
        public Error? Error { get; set; }
    }

    public class Error
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
    }
}
