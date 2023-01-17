using AndroidAppWear;
using AndroidAppWear.Interfaces;
using Org.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin;

[assembly: Xamarin.Forms.Dependency(typeof(FileService))]
namespace AndroidAppWear
{
    internal class FileService : IFileService
    {
        int i = 0;
        string filename = "json_file.json";

        public string Filename { get => filename; }

        public string GetRootPath()
        {
            return Application.Context.GetExternalFilesDir(null).ToString();
        }

        public string GetFullPath()
        {
            return GetRootPath() + "/" + filename;
        }

        public void CreateFile(string textJ)
        {
            var destination = Path.Combine(GetRootPath(), ++i + filename);
            File.WriteAllText(destination, textJ);
            
        }
    }
}
