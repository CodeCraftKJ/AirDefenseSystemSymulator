using System;
using System.Numerics;
using System.Threading.Tasks;

namespace AirDefenseSystem.Core.Models
{
    // Metaprogramowanie: Atrybut [Serializable] pozwala na serializację obiektu
    [Serializable]
    public class Target
    {
        public int Id { get; }
        public Vector3 Position { get; private set; }
        public Vector3 Velocity { get; private set; }
        public bool IsDestroyed { get; set; }
        public int TurnsAfterDestruction { get; set; }

        public Target(int id, Vector3 position, Vector3 velocity)
        {
            Id = id;
            Position = position;
            Velocity = velocity;
            IsDestroyed = false;
            TurnsAfterDestruction = 0;
        }

        public void UpdatePosition()
        {
            if (IsDestroyed) return;

            // Aktualizacja pozycji na podstawie prędkości
            Position += Velocity;

            // Odbijanie od granic (zakres -100 do 100)
            if (Math.Abs(Position.X) > 100)
            {
                Velocity = new Vector3(-Velocity.X, Velocity.Y, Velocity.Z);
                Position = new Vector3(Math.Sign(Position.X) * 100, Position.Y, Position.Z);
            }
            if (Math.Abs(Position.Z) > 100)
            {
                Velocity = new Vector3(Velocity.X, Velocity.Y, -Velocity.Z);
                Position = new Vector3(Position.X, Position.Y, Math.Sign(Position.Z) * 100);
            }
        }

        // Metaprogramowanie: Metoda fabryczna do tworzenia obiektów
        public static Target CreateRandom(int id)
        {
            var random = new Random();
            
            // Losowa pozycja w zakresie -100 do 100
            Vector3 position = new Vector3(
                (float)(random.NextDouble() * 200 - 100),
                0,
                (float)(random.NextDouble() * 200 - 100)
            );

            // Losowa prędkość w zakresie -5 do 5
            Vector3 velocity = new Vector3(
                (float)(random.NextDouble() * 10 - 5),
                0,
                (float)(random.NextDouble() * 10 - 5)
            );

            return new Target(id, position, velocity);
        }

        // Asynchroniczna aktualizacja pozycji
        public async Task UpdatePositionAsync()
        {
            if (IsDestroyed) return;

            // Aktualizacja pozycji na podstawie prędkości
            Position += Velocity * 0.1f; // 0.1s krok czasowy

            // Symulacja małych zmian kierunku (turbulencje)
            var random = new Random();
            if (random.NextDouble() < 0.1) // 10% szans na zmianę kierunku
            {
                float angleChange = (float)((random.NextDouble() - 0.5) * 0.1); // Maksymalna zmiana 5.7 stopnia
                float elevationChange = (float)((random.NextDouble() - 0.5) * 0.1);

                // Obliczenie nowego kierunku
                float currentAngle = (float)Math.Atan2(Velocity.Z, Velocity.X);
                float currentElevation = (float)Math.Asin(Velocity.Y / 5);

                float newAngle = currentAngle + angleChange;
                float newElevation = (float)Math.Clamp(currentElevation + elevationChange, -Math.PI/4, Math.PI/4);

                // Aktualizacja wektora prędkości
                Velocity = new Vector3(
                    (float)(Math.Cos(newAngle) * Math.Cos(newElevation) * 5),
                    (float)(Math.Sin(newElevation) * 5),
                    (float)(Math.Sin(newAngle) * Math.Cos(newElevation) * 5)
                );
            }

            await Task.Delay(100); // Symulacja czasu potrzebnego na aktualizację
        }
    }
} 