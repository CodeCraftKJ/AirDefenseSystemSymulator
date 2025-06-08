using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using AirDefenseSystem.Core.Models;
using AirDefenseSystem.Core.Utils;

namespace AirDefenseSystem.Core.Systems
{
    public class AirDefenseSystem : IDisposable
    {
        private readonly ILogger _logger;
        private readonly RadarSystem _radar;
        private readonly Dictionary<int, Target> _targets;
        private readonly Random _random;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning;
        private int _nextTargetId = 1;
        private const float HIT_CHANCE = 0.33f; // 33% szans na trafienie
        private const int MAX_TARGETS = 20; // Maksymalna liczba celów
        private const float SPAWN_CHANCE = 0.7f; // 70% szans na pojawienie się nowego celu w każdej iteracji
        private Target _currentlyEngagedTarget;

        public AirDefenseSystem(ILogger logger)
        {
            _logger = logger;
            _radar = new RadarSystem(new Vector3(0, 0, 0), 100000); // 100km range
            _targets = new Dictionary<int, Target>();
            _random = new Random();
            _cancellationTokenSource = new CancellationTokenSource();
            _currentlyEngagedTarget = null;
        }

        public RadarSystem Radar => _radar;
        public Dictionary<int, Target> Targets => _targets;

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            _logger.LogSystemStart(0);
            Task.Run(async () => await RunSystemLoop(_cancellationTokenSource.Token));
        }

        public void Stop()
        {
            if (!_isRunning) return;
            _isRunning = false;
            _cancellationTokenSource.Cancel();
            _logger.LogSystemStop();
        }

        private async Task RunSystemLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Spawn nowych celów
                    if (_targets.Count < MAX_TARGETS && _random.NextDouble() < SPAWN_CHANCE)
                    {
                        SpawnNewTarget();
                    }

                    // Aktualizacja pozycji celów
                    var updateTasks = _targets.Values
                        .Where(t => !t.IsDestroyed)
                        .Select(target => target.UpdatePositionAsync())
                        .ToList();
                    await Task.WhenAll(updateTasks);

                    // Usuwanie celów, które wyleciały poza zasięg
                    RemoveOutOfRangeTargets();

                    // Skanowanie radaru
                    var readings = await _radar.ScanAsync(_targets);
                    var priorityTargets = _radar.GetPriorityTargets();
                    _logger.LogRadarStatus(readings, priorityTargets, _radar);

                    // Angażowanie celów
                    if (_currentlyEngagedTarget == null || _currentlyEngagedTarget.IsDestroyed)
                    {
                        var nextTarget = priorityTargets
                            .Where(reading => _targets.TryGetValue(reading.TargetId, out var target) && !target.IsDestroyed)
                            .Select(reading => _targets[reading.TargetId])
                            .FirstOrDefault();

                        if (nextTarget != null)
                        {
                            _currentlyEngagedTarget = nextTarget;
                            await EngageTargetAsync(nextTarget);
                        }
                    }

                    await Task.Delay(1000, cancellationToken);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
            }
        }

        private void SpawnNewTarget()
        {
            var target = Target.CreateRandom(_nextTargetId++, 100, 1000, 100000); // 100km spawn range
            _targets[target.Id] = target;
            _logger.LogTargetTracking(target);
        }

        private void RemoveOutOfRangeTargets()
        {
            var outOfRange = _targets.Values
                .Where(t => !t.IsDestroyed && Vector3.Distance(_radar.Position, t.Position) > _radar.Range * 1.5f)
                .ToList();

            foreach (var target in outOfRange)
            {
                _targets.Remove(target.Id);
                if (target == _currentlyEngagedTarget)
                {
                    _currentlyEngagedTarget = null;
                }
                Console.WriteLine($"Target {target.Id} left the area");
            }
        }

        private async Task EngageTargetAsync(Target target)
        {
            _logger.LogEngagementStart(target);
            await Task.Delay(2000); // Symulacja czasu potrzebnego na strzał

            if (_random.NextDouble() < HIT_CHANCE)
            {
                target.IsDestroyed = true;
                _currentlyEngagedTarget = null;
                _logger.LogTargetDestroyed(target);
            }
            else
            {
                Console.WriteLine($"Missed target {target.Id}");
            }
        }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSource.Dispose();
        }
    }
} 