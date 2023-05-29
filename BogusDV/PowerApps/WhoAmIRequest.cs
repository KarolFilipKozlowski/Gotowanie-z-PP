using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BogusDV.PowerApps
{
    /// <summary>
    /// Contains the data to perform the WhoAmI function
    /// </summary>
    public sealed class WhoAmIRequest : HttpRequestMessage
    {
        /// <summary>
        /// Initializes the WhoAmIRequest
        /// </summary>
        public WhoAmIRequest()
        {
            Method = HttpMethod.Get;
            RequestUri = new Uri(
                uriString: "WhoAmI",
                uriKind: UriKind.Relative);
        }
    }
}
