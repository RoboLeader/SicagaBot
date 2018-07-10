using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


/* TODO:
 * Update. We don't have to rely on dotnet core 1.0 anymore.
 */
namespace Sicagabot.Services
{
    class DownloadFile
    {
        //uses HttpClient to maintain compatability with dotnet Core 1.0
        public static async Task<byte[]> DownloadAsByteArray(string url)
        {
            using (var client = new HttpClient())
            {

                using (var result = await client.GetAsync(url))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        return await result.Content.ReadAsByteArrayAsync();
                    }

                }
            }
            return null;
        }
    }
}
