using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MonolithicExtensions.General
{
    /// <summary>
    /// Extension functions for HttpClient objects.
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// A simple extension to simplify the process of posting string data and obtaining a string result using an HttpClient
        /// </summary>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<string> PostStringAsync(this HttpClient client, string uri, string data)
        {
            var result = await client.PostAsync(uri, new StringContent(data));
            var content = await result.Content.ReadAsStringAsync();
            return content;
        }
    }
}
