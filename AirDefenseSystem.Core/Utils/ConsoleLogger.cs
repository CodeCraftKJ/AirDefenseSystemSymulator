using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using AirDefenseSystem.Core.Models;
using AirDefenseSystem.Core.Systems;

namespace AirDefenseSystem.Core.Utils
{
    public class ConsoleLogger : ILogger
    {
        public void LogSystemStart(int targetCount)
        {
            Console.WriteLine($"System started with {targetCount} targets");
        }

        public void LogSystemStop()
        {
            Console.WriteLine("System stopped");
        }

        public void LogRadarStatus(List<RadarReading> readings, List<RadarReading> priorityTargets, RadarSystem radar)
        {
            Console.WriteLine($"\nRadar Status: {readings.Count} targets detected, {priorityTargets.Count} priority targets");
        }

        public void LogTargetTracking(Target target)
        {
            Console.WriteLine($"Tracking target {target.Id} at position X={target.Position.X/1000:F1}km, Y={target.Position.Y/1000:F1}km, Z={target.Position.Z/1000:F1}km");
        }

        public void LogEngagementStart(Target target)
        {
            Console.WriteLine($"Engaging target {target.Id} at speed {00000000:F1}m/s");
        }

        public void LogTargetDestroyed(Target target)
        {
            Console.WriteLine($"Target {target.Id} destroyed at position X={target.Position.X/1000:F1}km, Y={target.Position.Y/1000:F1}km, Z={target.Position.Z/1000:F1}km");
        }
    }
} 