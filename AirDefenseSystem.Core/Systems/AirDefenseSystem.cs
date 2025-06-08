using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using AirDefenseSystem.Core.Models;
using AirDefenseSystem.Core.Utils;
using System.Linq;

namespace AirDefenseSystem.Core.Systems
{
    public class AirDefenseSystem
    {
        private readonly RadarSystem _radar;
        private readonly ILogger _logger;
        private readonly Random _random;
        private readonly object _targetsLock = new object();
        private readonly ManualResetEventSlim _updateEvent;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _systemTask;
        private const float SPAWN_CHANCE = 0.7f;
        private const float HIT_CHANCE = 0.33f;
        private const int MAX_TARGETS = 20;

        public Dictionary<int, Target> Targets { get; } = new Dictionary<int, Target>();
        public bool IsRunning => _systemTask != null && !_systemTask.IsCompleted;
        public ManualResetEventSlim UpdateEvent => _updateEvent;

        public AirDefenseSystem(RadarSystem radar, ILogger logger = null)
        {
            _radar = radar;
            _logger = logger ?? new ConsoleLogger();
            _random = new Random();
            _updateEvent = new ManualResetEventSlim(false);
        }

        public async Task StartAsync()
        {
            if (IsRunning) return;

            _cancellationTokenSource = new CancellationTokenSource();
            _systemTask = Task.Run(async () =>
            {
                try
                {
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await UpdateTargetsAsync();
                        await ScanAndEngageAsync();
                        await Task.Delay(1000, _cancellationTokenSource.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Normalne zakończenie
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Błąd systemu: {ex.Message}");
                }
            }, _cancellationTokenSource.Token);

            await _systemTask;
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _systemTask?.Wait();
            _updateEvent.Set(); // Upewnij się, że ostatnia aktualizacja jest wyświetlona
        }

        private async Task UpdateTargetsAsync()
        {
            // Usuń zniszczone cele
            lock (_targetsLock)
            {
                var destroyedTargets = Targets.Values.Where(t => t.IsDestroyed).ToList();
                foreach (var target in destroyedTargets)
                {
                    Targets.Remove(target.Id);
                }
            }

            // Aktualizuj pozycje celów
            var updateTasks = new List<Task>();
            lock (_targetsLock)
            {
                foreach (var target in Targets.Values)
                {
                    updateTasks.Add(target.UpdatePositionAsync());
                }
            }
            await Task.WhenAll(updateTasks);

            // Sprawdź czy można dodać nowy cel
            if (_random.NextDouble() < SPAWN_CHANCE && Targets.Count < MAX_TARGETS)
            {
                var newTarget = Target.CreateRandom(_radar.Range, 0, 0);
                lock (_targetsLock)
                {
                    Targets[newTarget.Id] = newTarget;
                }
            }

            _updateEvent.Set();
        }

        private async Task ScanAndEngageAsync()
        {
            var readings = await _radar.ScanAsync(Targets.Values.ToList());
            if (readings.Count == 0) return;

            // Wybierz cel do zaatakowania (największy poziom zagrożenia)
            var targetToEngage = readings
                .OrderByDescending(r => r.ThreatLevel)
                .First();

            _logger.LogEngagementStart(targetToEngage.Target);

            // Symulacja strzału
            if (_random.NextDouble() < HIT_CHANCE)
            {
                targetToEngage.Target.IsDestroyed = true;
                _logger.LogTargetDestroyed(targetToEngage.Target);
            }
            else
            {
                _logger.LogEngagementMiss(targetToEngage.Target);
            }

            _updateEvent.Set();
        }
    }
} 