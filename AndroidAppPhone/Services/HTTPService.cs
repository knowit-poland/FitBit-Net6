using Android.Content;
using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidAppWear.Services
{
    [Service(Label = "HTTPService")]
    internal class HTTPService : Service
    {
        private const string adress =
            "https://inomo-inserter.azurewebsites.net/api/HttpTriggerSaveFitBitEntries?code=olLFi8x3Qf0y1nnXvDr2K9lAVWnNeChJYrd7g_AL3CfFAzFuirZMLw==";
        public async Task sendHTTPPost(string body)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(adress, content);
            }
        }
        public override IBinder? OnBind(Intent? intent)
        {
            return null;
        }
    }
}
