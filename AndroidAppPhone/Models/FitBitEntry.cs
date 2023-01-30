namespace FitbitInsertJson.Models
{
    public class FitBitEntry
    {
        public string id { get; set; } = string.Empty;
        public List<AudioData> AudioSamples { get; set; } = new List<AudioData>();
        public List<HRData> HRSamples { get; set; } = new List<HRData>();
        public List<GPSData> GPSSamples { get; set; } = new List<GPSData>();
        public string UserID { get; set; } = string.Empty;
    }
}
