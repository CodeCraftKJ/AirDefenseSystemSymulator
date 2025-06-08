using System.Numerics;

namespace AirDefenseSystem.Core.Models
{
    public class RadarReading
    {
        public Target Target { get; set; }
        public float Distance { get; set; }
        public float SignalStrength { get; set; }
        public float ThreatLevel { get; set; }
        public float PredictedDistance { get; set; }
    }
} 