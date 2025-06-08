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

        public void LogSystemStatus(string message)
        {
            System.Console.WriteLine($"[SYSTEM] {message}");
        }

        public void LogRadarStatus(List<RadarReading> readings, List<RadarReading> priorityTargets, RadarSystem radar)
        {
            System.Console.WriteLine($"\n=== STATUS RADARU ===");
            System.Console.WriteLine($"Zasięg: {radar.Range/1000:F1} km");
            System.Console.WriteLine($"Wykryte cele: {readings.Count}");
            
            foreach (var reading in readings)
            {
                var distance = Vector3.Distance(radar.Position, reading.Target.Position) / 1000f;
                System.Console.WriteLine($"\nCel {reading.Target.Id}:");
                System.Console.WriteLine($"  Pozycja: ({reading.Target.Position.X/1000:F1}, {reading.Target.Position.Y/1000:F1}, {reading.Target.Position.Z/1000:F1}) km");
                System.Console.WriteLine($"  Szybkość: {reading.Target.Speed:F1} m/s");
                System.Console.WriteLine($"  Odległość: {distance:F1} km");
                System.Console.WriteLine($"  Siła sygnału: {reading.SignalStrength:P0}");
                System.Console.WriteLine($"  Poziom zagrożenia: {reading.ThreatLevel:F1}");
                System.Console.WriteLine($"  Przewidywana odległość: {reading.PredictedDistance/1000:F1} km");
            }
        }

        public void LogTargetTracking(Target target, string message)
        {
            System.Console.WriteLine($"[ŚLEDZENIE] Cel {target.Id}: {message}");
        }

        public void LogEngagementStart(Target target)
        {
            System.Console.WriteLine($"[ATAK] Rozpoczęto atak na cel {target.Id}");
        }

        public void LogTargetDestroyed(Target target)
        {
            System.Console.WriteLine($"[ZESTRZELENIE] Cel {target.Id} został zniszczony!");
        }

        public void LogError(string message)
        {
            System.Console.WriteLine($"[BŁĄD] {message}");
        }

        public void LogEngagementMiss(Target target)
        {
            System.Console.WriteLine($"[CHYBIENIE] Chybiono cel {target.Id}");
        }
    }
} 