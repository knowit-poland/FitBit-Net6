using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidAppWear
{
    internal class GPSSamples
    {
        public string TimeStamp;
        public double Latitude;
        public double Longitude;

        public GPSSamples(string date, double latitude, double longitude)
        {
            this.TimeStamp = date;
            this.Latitude = latitude;
            this.Longitude = longitude;
        }   
    }
}
