using System.Collections.Generic;
using System.Numerics;
using AirDefenseSystem.Core.Models;
using AirDefenseSystem.Core.Systems;

namespace AirDefenseSystem.Core.Utils
{
    public interface ILogger
    {
        void LogSystemStart(int targetCount);
        void LogSystemStop();
        void LogSystemStatus(string status);
        void LogRadarStatus(List<RadarReading> readings, List<RadarReading> priorityTargets, RadarSystem radar);
        void LogTargetTracking(Target target, string message = "");
        void LogEngagementStart(Target target);
        void LogTargetDestroyed(Target target);
        void LogError(string message);
        void LogEngagementMiss(Target target);
    }
} 