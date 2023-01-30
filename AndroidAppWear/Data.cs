using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidAppWear
{
    internal class Data
    {
        public List<AudioSamples> AudioSamples { get; set; }
        public List<HrSamples> HRSamples { get; set; }
        public List<GPSSamples> GPSSamples { get; set; }
        public string UserID { get; set; }
        public string id { get; set; }
    }
}
