namespace AirDefenseSystem.Core.Models
{
    public class RadarReading
    {
        public float Distance { get; set; }
        public float SignalStrength { get; set; }
        public int TargetId { get; set; }
        public float ThreatLevel { get; set; }
        public float PredictedDistance { get; set; }
    }
} 