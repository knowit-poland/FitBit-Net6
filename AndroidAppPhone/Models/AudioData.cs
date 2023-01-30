using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitbitInsertJson.Models
{
    public class AudioData
    {
        public DateTime TimeStamp { get; set; }
        public string AudioSample { get; set; } = string.Empty;
        public string RecognizedText { get; set; } = string.Empty;

        public override string ToString()
        {
            var sample = AudioSample.Length > 3 ? AudioSample.Take(3) + " ... " + AudioSample.TakeLast(3) : string.Empty;
            return $"AudioSample:{sample} at {TimeStamp}";
        }

    }
}
