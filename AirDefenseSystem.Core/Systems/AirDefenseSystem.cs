using System;
using System.Threading;
using System.Threading.Tasks;
using AirDefenseSystem.Core.Utils;

namespace AirDefenseSystem.Core.Systems
{
    public class AirDefenseSystem : IDisposable
    {
        private readonly ILogger _logger;
        private readonly RadarSystem _radar;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning;

        public AirDefenseSystem(ILogger logger)
        {
            _logger = logger;
            _radar = new RadarSystem();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public RadarSystem Radar => _radar;

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
                    _radar.Update();
                    await Task.Delay(1000, cancellationToken);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
            }
        }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSource.Dispose();
        }
    }
} 