using System;
using System.Threading;
using System.Threading.Tasks;
using AirDefenseSystem.Core.Systems;
using AirDefenseSystem.Core.Utils;
using AirDefenseSystem.Console.Display;

namespace AirDefenseSystem.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var logger = new ConsoleLogger();
            var system = new AirDefenseSystem.Core.Systems.AirDefenseSystem(logger);
            var radarDisplay = new RadarDisplay();
            
            system.Start();

            System.Console.WriteLine("System obrony powietrznej uruchomiony.\nNaciśnij Ctrl+C aby zatrzymać system...\n");

            var cts = new CancellationTokenSource();
            System.Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            while (!cts.IsCancellationRequested)
            {
                radarDisplay.Update(system);
                await Task.Delay(1000);
            }

            system.Stop();
        }
    }
}
