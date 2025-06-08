using System;
using System.Threading;
using System.Threading.Tasks;
using AirDefenseSystem.Console.Display;

namespace AirDefenseSystem.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var radarDisplay = new RadarDisplay();
            
            System.Console.WriteLine("Test wyświetlania radaru.\nNaciśnij Ctrl+C aby zatrzymać...\n");

            var cts = new CancellationTokenSource();
            System.Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            while (!cts.IsCancellationRequested)
            {
                radarDisplay.Update();
                await Task.Delay(200); // Aktualizacja co 200ms dla płynniejszego ruchu
            }
        }
    }
}
