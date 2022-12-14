using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidAppWear
{
    internal class LocationSamples
    {
        public string date;
        public double latitute;
        public double longitute;

        public LocationSamples(string date, double latitute, double longitute)
        {
            this.date = date;
            this.latitute = latitute;
            this.longitute = longitute;
        }   
    }
}
