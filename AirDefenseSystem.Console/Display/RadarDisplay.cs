using System;
using System.Collections.Generic;
using System.Numerics;
using AirDefenseSystem.Core.Models;
using AirDefenseSystem.Core.Systems;

namespace AirDefenseSystem.Console.Display
{
    public class RadarDisplay
    {
        private readonly RadarSystem _radar;
        private readonly Dictionary<int, Target> _targets;
        private const int DISPLAY_WIDTH = 80;
        private const int DISPLAY_HEIGHT = 24;
        private const char TARGET_CHAR = '●';
        private const char RADAR_CHAR = '○';
        private const char EMPTY_CHAR = '·';
        private readonly char[,] _displayBuffer;

        public RadarDisplay(RadarSystem radar, Dictionary<int, Target> targets)
        {
            _radar = radar;
            _targets = targets;
            _displayBuffer = new char[DISPLAY_HEIGHT, DISPLAY_WIDTH];
            System.Console.SetWindowSize(DISPLAY_WIDTH + 1, DISPLAY_HEIGHT + 10);
            System.Console.SetBufferSize(DISPLAY_WIDTH + 1, DISPLAY_HEIGHT + 10);
        }

        public void Update()
        {
            // Wyczyść bufor
            for (int y = 0; y < DISPLAY_HEIGHT; y++)
            {
                for (int x = 0; x < DISPLAY_WIDTH; x++)
                {
                    _displayBuffer[y, x] = ' ';
                }
            }

            // Rysuj radar w buforze
            DrawRadarToBuffer();
            DrawTargetsToBuffer();

            // Wyczyść konsolę i ustaw kursor na początku
            System.Console.SetCursorPosition(0, 0);
            System.Console.Clear();

            // Wyświetl zawartość bufora
            for (int y = 0; y < DISPLAY_HEIGHT; y++)
            {
                for (int x = 0; x < DISPLAY_WIDTH; x++)
                {
                    System.Console.Write(_displayBuffer[y, x]);
                }
                System.Console.WriteLine();
            }

            // Rysuj legendę
            DrawLegend();
        }

        private void DrawRadarToBuffer()
        {
            for (int y = 0; y < DISPLAY_HEIGHT; y++)
            {
                for (int x = 0; x < DISPLAY_WIDTH; x++)
                {
                    float dx = (x - DISPLAY_WIDTH / 2) / (float)(DISPLAY_WIDTH / 2);
                    float dy = (y - DISPLAY_HEIGHT / 2) / (float)(DISPLAY_HEIGHT / 2);
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (distance <= 1.0f)
                    {
                        if (Math.Abs(distance - 0.25f) < 0.02f ||
                            Math.Abs(distance - 0.5f) < 0.02f ||
                            Math.Abs(distance - 0.75f) < 0.02f ||
                            Math.Abs(distance - 1.0f) < 0.02f)
                        {
                            _displayBuffer[y, x] = RADAR_CHAR;
                        }
                        else
                        {
                            _displayBuffer[y, x] = EMPTY_CHAR;
                        }
                    }
                }
            }
        }

        private void DrawTargetsToBuffer()
        {
            foreach (var target in _targets.Values)
            {
                if (target.IsDestroyed) continue;

                // Konwertuj pozycję 3D na 2D (widok z góry)
                float x = target.Position.X / _radar.Range;
                float y = target.Position.Z / _radar.Range; // Używamy Z zamiast Y dla widoku z góry

                // Przekształć współrzędne na ekran
                int screenX = (int)((x + 1) * DISPLAY_WIDTH / 2);
                int screenY = (int)((y + 1) * DISPLAY_HEIGHT / 2);

                // Sprawdź czy punkt jest na ekranie
                if (screenX >= 0 && screenX < DISPLAY_WIDTH && screenY >= 0 && screenY < DISPLAY_HEIGHT)
                {
                    _displayBuffer[screenY, screenX] = TARGET_CHAR;
                }
            }
        }

        private void DrawLegend()
        {
            System.Console.WriteLine("\nLegenda:");
            System.Console.WriteLine($"{TARGET_CHAR} - Cel");
            System.Console.WriteLine($"{RADAR_CHAR} - Okręgi radaru");
            System.Console.WriteLine($"{EMPTY_CHAR} - Obszar skanowania");
            System.Console.WriteLine($"Zasięg radaru: {_radar.Range/1000:F1} km");
            System.Console.WriteLine($"Liczba celów: {_targets.Count}");
        }
    }
} 