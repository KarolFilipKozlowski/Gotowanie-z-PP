using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BogusDV.PowerApps
{
    public static partial class Extensions
    {
        /// <summary>
        /// Converts HttpResponseMessage to derived type
        /// </summary>
        /// <typeparam name="T">The type derived from HttpResponseMessage</typeparam>
        /// <param name="response">The HttpResponseMessage</param>
        /// <returns></returns>
        public static T As<T>(this HttpResponseMessage response) where T : HttpResponseMessage
        {
            T? typedResponse = (T)Activator.CreateInstance(typeof(T));

            //Copy the properties
            typedResponse.StatusCode = response.StatusCode;
            response.Headers.ToList().ForEach(h => {
                typedResponse.Headers.TryAddWithoutValidation(h.Key, h.Value);
            });
            typedResponse.Content = response.Content;
            return typedResponse;
        }
    }
}
