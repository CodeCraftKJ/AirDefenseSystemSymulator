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
        private readonly Vector3 _position;
        private readonly float _range;
        private readonly Random _random;
        private readonly List<RadarReading> _detectedTargets;
        private readonly object _scanLock = new object();
        private readonly Dictionary<int, Vector3> _lastPositions = new Dictionary<int, Vector3>();
        private readonly Dictionary<int, float> _lastDistances = new Dictionary<int, float>();
        private readonly Dictionary<int, Target> _targets = new Dictionary<int, Target>();

        public Vector3 Position => _position;
        public float Range => _range;

        public RadarSystem(Vector3? position = null, float range = 100000f) // 100km domyślny zasięg
        {
            _position = position ?? Vector3.Zero;
            _range = range;
            _random = new Random();
            _detectedTargets = new List<RadarReading>();
        }

        public Target GetTarget(int targetId)
        {
            return _targets.TryGetValue(targetId, out var target) ? target : null;
        }

        public async Task<List<RadarReading>> ScanAsync(List<Target> targets)
        {
            var readings = new List<RadarReading>();
            var tasks = new List<Task>();

            foreach (var target in targets)
            {
                tasks.Add(Task.Run(async () =>
                {
                    // Symulacja opóźnienia skanowania
                    await Task.Delay(50);

                    var distance = Vector3.Distance(Position, target.Position);
                    if (distance <= Range)
                    {
                        // Obliczanie siły sygnału (im bliżej, tym silniejszy)
                        float signalStrength = 1.0f - (distance / Range);
                        
                        // Dodanie losowego szumu
                        signalStrength += (float)(_random.NextDouble() - 0.5) * 0.1f;
                        signalStrength = Math.Clamp(signalStrength, 0, 1);

                        // Obliczanie poziomu zagrożenia
                        float threatLevel = CalculateThreatLevel(target, distance);

                        // Obliczanie przewidywanej odległości
                        float predictedDistance = CalculatePredictedDistance(target);

                        var reading = new RadarReading
                        {
                            Target = target,
                            Distance = distance,
                            SignalStrength = signalStrength,
                            ThreatLevel = threatLevel,
                            PredictedDistance = predictedDistance
                        };

                        lock (_scanLock)
                        {
                            readings.Add(reading);
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return readings;
        }

        public List<RadarReading> GetPriorityTargets(float minPriority = 50)
        {
            lock (_scanLock)
            {
                return _detectedTargets
                    .Where(r => r.ThreatLevel >= minPriority)
                    .OrderByDescending(r => r.ThreatLevel)
                    .ToList();
            }
        }

        private float CalculateAzimuth(Vector3 targetPosition)
        {
            var direction = targetPosition - _position;
            return (float)Math.Atan2(direction.Z, direction.X);
        }

        private float CalculateElevation(Vector3 targetPosition)
        {
            var direction = targetPosition - _position;
            var horizontalDistance = (float)Math.Sqrt(direction.X * direction.X + direction.Z * direction.Z);
            return (float)Math.Atan2(direction.Y, horizontalDistance);
        }

        private float CalculateSignalStrength(float distance)
        {
            // Im większa odległość, tym słabszy sygnał
            float baseStrength = 100f * (1f - (distance / _range));
            // Dodanie losowego szumu
            float noise = (float)(_random.NextDouble() * 10 - 5);
            return Math.Clamp(baseStrength + noise, 0, 100);
        }

        private float CalculateThreatLevel(Target target, float distance)
        {
            // Podstawowy poziom zagrożenia zależny od odległości
            float baseThreat = 1.0f - (distance / _range);
            
            // Dodatkowe zagrożenie zależne od prędkości
            float speedFactor = target.Speed / 1000f; // Normalizacja prędkości
            
            // Łączny poziom zagrożenia
            float threatLevel = (baseThreat * 0.7f + speedFactor * 0.3f) * 100f;
            
            return Math.Clamp(threatLevel, 0, 100);
        }

        private float CalculatePredictedDistance(Target target)
        {
            // Przewidywana odległość za 5 sekund
            Vector3 predictedPosition = target.Position + target.Velocity * 5;
            return Vector3.Distance(_position, predictedPosition);
        }
    }
} 