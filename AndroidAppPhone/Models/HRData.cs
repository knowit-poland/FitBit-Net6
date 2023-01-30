namespace FitbitInsertJson.Models
{
    public class HRData
    {
        public DateTime TimeStamp { get; set; }
        public double HRValue { get; set; }

        public override string ToString()
        {
            return $"HR:{HRValue} at {TimeStamp}";
        }
    }

}
