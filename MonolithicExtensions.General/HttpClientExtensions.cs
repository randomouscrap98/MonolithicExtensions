using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MonolithicExtensions.General
{
    public static class HttpClientExtensions
    {
        public static async Task<string> PostStringAsync(this HttpClient client, string uri, string data)
        {
            var result = await client.PostAsync(uri, new StringContent(data));
            var content = await result.Content.ReadAsStringAsync();
            return content;
        }
    }
}
