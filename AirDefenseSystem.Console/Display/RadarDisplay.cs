using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using AirDefenseSystem.Core.Systems;
using AirDefenseSystem.Core.Models;
using AirDefenseSystem.Core.Utils;

namespace AirDefenseSystem.Console.Display
{
    public class RadarDisplay
    {
        private const int RADAR_SIZE = 21;
        private const int HALF_SIZE = RADAR_SIZE / 2;
        private readonly char[,] _radarGrid;
        private readonly List<string> _targetDetails;
        private int _updateCount;
        private readonly Core.Systems.AirDefenseSystem _airDefenseSystem;
        private readonly object _lockObject = new object();
        private bool _isUpdating;

        public RadarDisplay()
        {
            _radarGrid = new char[RADAR_SIZE, RADAR_SIZE];
            _targetDetails = new List<string>();
            _updateCount = 0;
            System.Console.CursorVisible = false;
            System.Console.SetCursorPosition(0, 0);

            // Inicjalizacja systemu obrony powietrznej
            var logger = new ConsoleLogger();
            _airDefenseSystem = new Core.Systems.AirDefenseSystem(logger);
            _airDefenseSystem.Start();

            // Uruchomienie nasłuchiwania klawiszy w tle
            Task.Run(CheckForTargetDestruction);
        }

        private async Task CheckForTargetDestruction()
        {
            while (true)
            {
                if (System.Console.KeyAvailable)
                {
                    var key = System.Console.ReadKey(true);
                    if (char.IsDigit(key.KeyChar))
                    {
                        int targetIndex = key.KeyChar - '0';
                        _airDefenseSystem.Radar.DestroyTarget(targetIndex);
                        System.Console.Beep(1000, 200); // Dźwięk zestrzelenia
                    }
                }
                await Task.Delay(50); // Sprawdzanie co 50ms
            }
        }

        public void Update()
        {
            if (_isUpdating) return; // Zabezpieczenie przed równoległymi aktualizacjami

            lock (_lockObject)
            {
                _isUpdating = true;
                try
                {
                    // Resetowanie siatki
                    for (int y = 0; y < RADAR_SIZE; y++)
                        for (int x = 0; x < RADAR_SIZE; x++)
                            _radarGrid[y, x] = '.';

                    // Umieszczenie radaru w środku
                    _radarGrid[HALF_SIZE, HALF_SIZE] = 'R';

                    // Aktualizacja szczegółów celów
                    _targetDetails.Clear();
                    var activeTargets = _airDefenseSystem.Radar.Targets.Where(t => !t.IsDestroyed).ToList();
                    var destroyedTargets = _airDefenseSystem.Radar.Targets.Where(t => t.IsDestroyed).ToList();

                    // Najpierw wyświetl aktywne cele
                    foreach (var target in activeTargets)
                    {
                        // Umieszczenie celu na siatce
                        int gridX = (int)((target.Position.X / 100) * HALF_SIZE) + HALF_SIZE;
                        int gridY = (int)((target.Position.Z / 100) * HALF_SIZE) + HALF_SIZE;

                        if (gridX >= 0 && gridX < RADAR_SIZE && gridY >= 0 && gridY < RADAR_SIZE)
                        {
                            _radarGrid[gridY, gridX] = (char)('0' + (target.Id % 10));
                        }

                        string position = $"P:({target.Position.X:F1}, {target.Position.Y:F1}, {target.Position.Z:F1})";
                        string velocity = $"V:({target.Velocity.X:F1}, {target.Velocity.Y:F1}, {target.Velocity.Z:F1})";
                        _targetDetails.Add($"Target {target.Id}: {position} {velocity}");
                    }

                    // Potem wyświetl zestrzelone cele
                    foreach (var target in destroyedTargets)
                    {
                        // Umieszczenie celu na siatce
                        int gridX = (int)((target.Position.X / 100) * HALF_SIZE) + HALF_SIZE;
                        int gridY = (int)((target.Position.Z / 100) * HALF_SIZE) + HALF_SIZE;

                        if (gridX >= 0 && gridX < RADAR_SIZE && gridY >= 0 && gridY < RADAR_SIZE)
                        {
                            _radarGrid[gridY, gridX] = 'X';
                        }

                        string position = $"P:({target.Position.X:F1}, {target.Position.Y:F1}, {target.Position.Z:F1})";
                        string velocity = $"V:({target.Velocity.X:F1}, {target.Velocity.Y:F1}, {target.Velocity.Z:F1})";
                        _targetDetails.Add($"Target {target.Id}: {position} {velocity} [ZESTRZELONY - zniknie za {target.TurnsAfterDestruction} tur]");
                    }

                    // Wyświetlenie aktualnego stanu
                    DrawCurrentState();
                    _updateCount++;
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        private void DrawCurrentState()
        {
            // Ustawienie kursora na początek
            System.Console.SetCursorPosition(0, 0);
            
            // Wyświetlenie nagłówka
            System.Console.WriteLine($"Radar Display #{_updateCount} (R = Radar, 0-9 = Target, X = Destroyed):");
            System.Console.WriteLine("Naciśnij cyfrę 0-4 aby zestrzelić cel (0 = pierwszy cel, 1 = drugi cel, itd.)");
            
            // Rysowanie siatki radaru
            for (int y = 0; y < RADAR_SIZE; y++)
            {
                for (int x = 0; x < RADAR_SIZE; x++)
                {
                    System.Console.Write(_radarGrid[y, x] + " ");
                }
                System.Console.WriteLine();
            }

            // Wyświetlenie szczegółów celów
            foreach (var detail in _targetDetails)
            {
                System.Console.WriteLine(detail);
            }
            
            // Wyczyszczenie pozostałej części ekranu
            int currentLine = System.Console.CursorTop;
            while (currentLine < System.Console.WindowHeight - 1)
            {
                System.Console.WriteLine(new string(' ', System.Console.WindowWidth - 1));
                currentLine++;
            }
        }

        public void Dispose()
        {
            _airDefenseSystem.Dispose();
        }
    }
} 