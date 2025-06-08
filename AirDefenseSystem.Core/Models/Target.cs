using System;
using System.Numerics;
using System.Threading.Tasks;

namespace AirDefenseSystem.Core.Models
{
    // Metaprogramowanie: Atrybut [Serializable] pozwala na serializację obiektu
    [Serializable]
    public class Target
    {
        private static int _nextId = 1;
        private readonly Random _random = new Random();

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
            Velocity = Vector3.Zero;
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
        public static Target CreateRandom(float spawnRange, float minSpeed = 100f, float maxSpeed = 1000f)
        {
            var random = new Random();
            var id = _nextId++;
            
            // Losowa pozycja w zasięgu spawnu
            var position = new Vector3(
                (float)(random.NextDouble() * spawnRange * 2 - spawnRange),
                (float)(random.NextDouble() * spawnRange * 2 - spawnRange),
                (float)(random.NextDouble() * spawnRange * 2 - spawnRange)
            );

            // Losowa prędkość w zakresie
            var speed = minSpeed + (float)(random.NextDouble() * (maxSpeed - minSpeed));

            return new Target(id, position, speed);
        }

        // Asynchroniczna aktualizacja pozycji
        public async Task UpdatePositionAsync()
        {
            if (IsDestroyed) return;

            // Symulacja opóźnienia aktualizacji
            await Task.Delay(100);

            // Losowa zmiana kierunku
            var newVelocity = new Vector3(
                (float)(_random.NextDouble() * 2 - 1),
                (float)(_random.NextDouble() * 2 - 1),
                (float)(_random.NextDouble() * 2 - 1)
            );

            // Normalizacja i skalowanie prędkości
            if (newVelocity != Vector3.Zero)
            {
                newVelocity = Vector3.Normalize(newVelocity) * Speed;
            }

            Velocity = newVelocity;
            Position += Velocity;
        }
    }
} 