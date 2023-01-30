using Android.Content;
using Android.OS;
using FitbitInsertJson.Models;
using Newtonsoft.Json;

namespace AndroidAppWear.Services
{
    [Service(Label = "HTTPService")]
    internal class HTTPService : Service
    {
        
        public async Task sendHTTPPost(int it)
        {
            try
            {
                for (int i = 1; i <= it; i++)
                {
                    string jsonAsString = null;
                    var path = Application.Context.GetExternalFilesDir(null).ToString() + "/jsonFile" + i + ".json";
                    System.Diagnostics.Debug.WriteLine(path);
                    
                    if (File.Exists(path))
                    {
                        jsonAsString = File.ReadAllText(path);
                    }
                    else
                        System.Diagnostics.Debug.WriteLine("file not founded");


                    
                    using var client = new HttpClient();
                    const string url = "https://inomo-inserter.azurewebsites.net/api/HttpTriggerSaveFitBitEntries?code=olLFi8x3Qf0y1nnXvDr2K9lAVWnNeChJYrd7g_AL3CfFAzFuirZMLw==";
                    var stringContent = new StringContent(jsonAsString);
                    System.Diagnostics.Debug.WriteLine(stringContent.ToString());
                    var response = await client.PostAsync(url, stringContent);
                    var body = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine(body);
                    File.Delete(path);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Something wrong" + ex.Message);
            }
        }
        public override IBinder? OnBind(Intent? intent)
        {
            return null;
        }
    }
}
