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
        public float Speed { get; }
        public bool IsTracked { get; set; }
        public bool IsEngaged { get; set; }
        public bool IsDestroyed { get; set; }
        public DateTime LastRadarReading { get; set; }

        public Target(int id, Vector3 position, float speed)
        {
            Id = id;
            Position = position;
            Speed = speed;
            IsTracked = false;
            IsEngaged = false;
            IsDestroyed = false;
            LastRadarReading = DateTime.Now;

            // Inicjalizacja losowego kierunku ruchu
            var random = new Random();
            float angle = (float)(random.NextDouble() * 2 * Math.PI);
            float elevation = (float)(random.NextDouble() * Math.PI / 4); // Maksymalne wznoszenie/opadanie 45 stopni
            
            // Konwersja kątów na wektor prędkości
            Velocity = new Vector3(
                (float)(Math.Cos(angle) * Math.Cos(elevation) * speed),
                (float)(Math.Sin(elevation) * speed),
                (float)(Math.Sin(angle) * Math.Cos(elevation) * speed)
            );
        }

        // Programowanie współbieżne: Metoda jest bezpieczna do wywoływania z wielu wątków
        public void UpdatePosition()
        {
            if (IsDestroyed) return;

            var random = new Random();
            // Różne wzorce ruchu w zależności od typu celu
            Vector3 velocity;
            velocity = new Vector3(
                (float)(random.NextDouble() - 0.5) * Speed,
                (float)(random.NextDouble() - 0.5) * Speed,
                (float)(random.NextDouble() - 0.5) * Speed
            );

            Position += velocity;
        }

        // Metaprogramowanie: Metoda fabryczna do tworzenia obiektów
        public static Target CreateRandom(int id, float minSpeed, float maxSpeed, float spawnRange)
        {
            var random = new Random();
            float speed = minSpeed + (float)(random.NextDouble() * (maxSpeed - minSpeed));
            
            // Losowa pozycja w sferze o promieniu spawnRange
            float theta = (float)(random.NextDouble() * 2 * Math.PI);
            float phi = (float)(random.NextDouble() * Math.PI);
            float r = (float)(random.NextDouble() * spawnRange);

            Vector3 position = new Vector3(
                (float)(r * Math.Sin(phi) * Math.Cos(theta)),
                (float)(r * Math.Sin(phi) * Math.Sin(theta)),
                (float)(r * Math.Cos(phi))
            );

            return new Target(id, position, speed);
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
                float currentElevation = (float)Math.Asin(Velocity.Y / Speed);

                float newAngle = currentAngle + angleChange;
                float newElevation = (float)Math.Clamp(currentElevation + elevationChange, -Math.PI/4, Math.PI/4);

                // Aktualizacja wektora prędkości
                Velocity = new Vector3(
                    (float)(Math.Cos(newAngle) * Math.Cos(newElevation) * Speed),
                    (float)(Math.Sin(newElevation) * Speed),
                    (float)(Math.Sin(newAngle) * Math.Cos(newElevation) * Speed)
                );
            }

            await Task.Delay(100); // Symulacja czasu potrzebnego na aktualizację
        }
    }
} 