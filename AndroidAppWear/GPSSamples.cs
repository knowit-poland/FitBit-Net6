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
        public double Latitute;
        public double Longitute;

        public GPSSamples(string date, double latitute, double longitute)
        {
            this.TimeStamp = date;
            this.Latitute = latitute;
            this.Longitute = longitute;
        }   
    }
}
