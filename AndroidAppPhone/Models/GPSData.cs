namespace FitbitInsertJson.Models
{
    public class GPSData
    {
        public DateTime TimeStamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public override string ToString()
        {
            return $"Latitude:{Latitude} and Longitude:{Longitude} at {TimeStamp}";
        }
    }

}
