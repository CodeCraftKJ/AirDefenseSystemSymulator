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
        void LogRadarStatus(List<RadarReading> readings, List<RadarReading> priorityTargets, RadarSystem radar);
        void LogTargetTracking(Target target);
        void LogEngagementStart(Target target);
        void LogTargetDestroyed(Target target);
    }
} 