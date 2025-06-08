using System;
using System.Linq;
using System.Collections.Generic;
using AirDefenseSystem.Core.Systems;
using AirDefenseSystem.Core.Models;

namespace AirDefenseSystem.Console.Display
{
    public class RadarDisplay
    {
        private const int RADAR_SIZE = 21;
        private const int HALF_SIZE = RADAR_SIZE / 2;
        private readonly char[,] _radarGrid;
        private readonly List<string> _targetDetails;
        private int _updateCount;

        public RadarDisplay()
        {
            _radarGrid = new char[RADAR_SIZE, RADAR_SIZE];
            _targetDetails = new List<string>();
            _updateCount = 0;
            System.Console.CursorVisible = false;
        }

        public void Update(AirDefenseSystem.Core.Systems.AirDefenseSystem system)
        {
            var radar = system.Radar;
            var readings = radar.ScanAsync(system.Targets).Result;

            // Resetowanie siatki
            for (int y = 0; y < RADAR_SIZE; y++)
                for (int x = 0; x < RADAR_SIZE; x++)
                    _radarGrid[y, x] = '.';

            // Umieszczenie radaru w środku
            _radarGrid[HALF_SIZE, HALF_SIZE] = 'R';

            // Umieszczenie celów na siatce
            foreach (var reading in readings)
            {
                var target = radar.GetTarget(reading.TargetId);
                if (target != null)
                {
                    int gridX = (int)((target.Position.X / radar.Range) * HALF_SIZE) + HALF_SIZE;
                    int gridY = (int)((target.Position.Z / radar.Range) * HALF_SIZE) + HALF_SIZE;
                    if (gridX >= 0 && gridX < RADAR_SIZE && gridY >= 0 && gridY < RADAR_SIZE)
                    {
                        if (target.IsDestroyed)
                            _radarGrid[gridY, gridX] = 'X';
                        else
                            _radarGrid[gridY, gridX] = (char)('0' + (reading.TargetId % 10));
                    }
                }
            }

            // Aktualizacja szczegółów celów
            _targetDetails.Clear();
            foreach (var reading in readings.OrderByDescending(r => r.ThreatLevel))
            {
                var target = radar.GetTarget(reading.TargetId);
                if (target != null)
                {
                    string approach = reading.PredictedDistance < reading.Distance ? "APPROACHING" : "MOVING AWAY";
                    string velocity = $"V:({target.Velocity.X:F1}, {target.Velocity.Y:F1}, {target.Velocity.Z:F1})";
                    string position = $"P:({target.Position.X/1000:F1}, {target.Position.Y/1000:F1}, {target.Position.Z/1000:F1})";
                    string speed = $"S:{target.Speed:F1}m/s";
                    
                    _targetDetails.Add($"Target {reading.TargetId}: {position} {velocity} {speed} " +
                                    $"Threat={reading.ThreatLevel:F1}%, {approach}");
                }
            }

            // Wyświetlenie aktualnego stanu
            DrawCurrentState();
            _updateCount++;
        }

        private void DrawCurrentState()
        {
            // Wyświetlenie nagłówka z numerem aktualizacji
            System.Console.WriteLine($"\nRadar Display #{_updateCount} (R = Radar, 0-9 = Target, X = Destroyed):");
            
            // Rysowanie siatki radaru
            for (int y = 0; y < RADAR_SIZE; y++)
            {
                for (int x = 0; x < RADAR_SIZE; x++)
                {
                    System.Console.Write(_radarGrid[y, x] + " ");
                }
                System.Console.WriteLine();
            }

            System.Console.WriteLine();
            
            // Wyświetlenie szczegółów celów
            foreach (var detail in _targetDetails)
            {
                System.Console.WriteLine(detail);
            }

            // Dodanie linii oddzielającej
            System.Console.WriteLine(new string('-', System.Console.WindowWidth - 1));
        }
    }
} 