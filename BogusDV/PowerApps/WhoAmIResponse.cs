using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BogusDV.PowerApps
{
    // This class must be instantiated by either:
    // - The Service.SendAsync<T> method
    // - The HttpResponseMessage.As<T> extension in Extensions.cs

    /// <summary>
    /// Contains the response from the WhoAmIRequest
    /// </summary>
    public sealed class WhoAmIResponse : HttpResponseMessage
    {

        // Cache the async content
        private string? _content;

        //Provides JObject for property getters
        private JObject _jObject
        {
            get
            {
                _content ??= Content.ReadAsStringAsync().GetAwaiter().GetResult();

                return JObject.Parse(_content);
            }
        }

        /// <summary>
        /// Gets the ID of the business to which the logged on user belongs.
        /// </summary>
        public Guid BusinessUnitId => (Guid)_jObject.GetValue(nameof(BusinessUnitId));

        /// <summary>
        /// Gets ID of the user who is logged on.
        /// </summary>
        public Guid UserId => (Guid)_jObject.GetValue(nameof(UserId));

        /// <summary>
        /// Gets ID of the organization that the user belongs to.
        /// </summary>
        public Guid OrganizationId => (Guid)_jObject.GetValue(nameof(OrganizationId));
    }
}