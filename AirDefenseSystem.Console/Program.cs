using System;
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
            System.Console.CursorVisible = false;
            System.Console.Title = "System Obrony Powietrznej";

            var radar = new RadarSystem();
            var logger = new ConsoleLogger();
            var system = new AirDefenseSystem.Core.Systems.AirDefenseSystem(radar, logger);
            var display = new RadarDisplay(radar, system.Targets);

            System.Console.WriteLine("Inicjalizacja systemu...");
            System.Console.WriteLine("Naciśnij Enter, aby zakończyć...");
            System.Console.ReadLine();

            // Uruchomienie systemu w tle
            var systemTask = system.StartAsync();

            try
            {
                while (!systemTask.IsCompleted)
                {
                    // Czekaj na sygnał aktualizacji
                    system.UpdateEvent.Wait();
                    
                    // Aktualizuj wyświetlanie
                    display.Update();
                    
                    // Resetuj sygnał
                    system.UpdateEvent.Reset();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Błąd: {ex.Message}");
            }
            finally
            {
                system.Stop();
                System.Console.CursorVisible = true;
            }
        }
    }
}
