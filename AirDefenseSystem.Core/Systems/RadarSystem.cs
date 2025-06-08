using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Linq;
using AirDefenseSystem.Core.Models;

namespace AirDefenseSystem.Core.Systems
{
    public class RadarSystem
    {
        public Vector3 Position { get; }
        public float Range { get; }
        private readonly List<RadarReading> _detectedTargets;
        private readonly object _scanLock = new object();
        private readonly Dictionary<int, Vector3> _lastPositions = new Dictionary<int, Vector3>();
        private readonly Dictionary<int, float> _lastDistances = new Dictionary<int, float>();
        private readonly Dictionary<int, Target> _targets = new Dictionary<int, Target>();

        public RadarSystem(Vector3 position, float range)
        {
            Position = position;
            Range = range;
            _detectedTargets = new List<RadarReading>();
        }

        public Target GetTarget(int targetId)
        {
            return _targets.TryGetValue(targetId, out var target) ? target : null;
        }

        public async Task<List<RadarReading>> ScanAsync(Dictionary<int, Target> targets)
        {
            _detectedTargets.Clear();
            _targets.Clear();
            foreach (var target in targets)
            {
                _targets[target.Key] = target.Value;
            }

            var tasks = new List<Task>();

            foreach (var target in targets.Values)
            {
                if (target.IsDestroyed) continue;
                tasks.Add(Task.Run(async () =>
                {
                    var distance = Vector3.Distance(Position, target.Position);
                    if (distance <= Range)
                    {
                        // Obliczanie przewidywanej odległości
                        float predictedDistance = distance;
                        if (_lastPositions.ContainsKey(target.Id))
                        {
                            var lastPos = _lastPositions[target.Id];
                            var movement = target.Position - lastPos;
                            var predictedPosition = target.Position + movement;
                            predictedDistance = Vector3.Distance(Position, predictedPosition);
                        }

                        // Obliczanie poziomu zagrożenia
                        float threatLevel = CalculateThreatLevel(distance, predictedDistance, target.Speed);

                        var reading = new RadarReading
                        {
                            Distance = distance,
                            SignalStrength = 100 * (1 - distance / Range),
                            TargetId = target.Id,
                            ThreatLevel = threatLevel,
                            PredictedDistance = predictedDistance
                        };

                        lock (_scanLock)
                        {
                            _detectedTargets.Add(reading);
                            _lastPositions[target.Id] = target.Position;
                            _lastDistances[target.Id] = distance;
                        }
                    }
                    await Task.Delay(50);
                }));
            }

            await Task.WhenAll(tasks);
            return _detectedTargets;
        }

        private float CalculateThreatLevel(float currentDistance, float predictedDistance, float speed)
        {
            // Im bliżej, tym większe zagrożenie
            float distanceThreat = 100 * (1 - currentDistance / Range);
            
            // Im szybciej się zbliża, tym większe zagrożenie
            float approachThreat = 0;
            if (predictedDistance < currentDistance)
            {
                approachThreat = 50 * (currentDistance - predictedDistance) / currentDistance;
            }

            // Im większa prędkość, tym większe zagrożenie
            float speedThreat = 50 * (speed / 1000); // Normalizacja prędkości

            return distanceThreat + approachThreat + speedThreat;
        }

        public List<RadarReading> GetPriorityTargets(float minPriority = 50)
        {
            return _detectedTargets
                .Where(r => r.ThreatLevel >= minPriority)
                .OrderByDescending(r => r.ThreatLevel)
                .ToList();
        }
    }
} 