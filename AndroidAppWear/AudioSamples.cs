using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidAppWear
{
    internal class AudioSamples
    {
        public string TimeStamp;
        public string AudioSample;

        public AudioSamples(string date, string audioSample)
        {
            this.TimeStamp = date;
            this.AudioSample = audioSample;
        }
    }
}
