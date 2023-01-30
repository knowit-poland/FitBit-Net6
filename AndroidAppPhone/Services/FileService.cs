using Android.Content;
using Android.OS;
using AndroidAppWear;
using Org.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin;


namespace AndroidAppWear
{
    [Service(Label = "FileService")]
    internal class FileService : Service
    {
        public string GetRootPath()
        {
            return Application.Context.GetExternalFilesDir(null).ToString();
        }
        public void CreateFile(string textJ, string fileName)
        {
            var destination = Path.Combine(GetRootPath(), fileName);
            File.WriteAllText(destination, textJ);
            
        }

        public override IBinder? OnBind(Intent? intent)
        {
            return null;
        }
    }
}
