using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Linq;
using AirDefenseSystem.Core.Models;

namespace AirDefenseSystem.Core.Systems
{
    public class RadarSystem
    {
        private readonly List<Target> _targets;
        private readonly object _lockObject = new object();
        private int _nextTargetId = 1;
        private const int MAX_TARGETS = 5;
        private const float SPAWN_CHANCE = 0.1f; // 10% szans na pojawienie się nowego celu w każdej iteracji

        public RadarSystem()
        {
            _targets = new List<Target>();
        }

        public IReadOnlyList<Target> Targets => _targets;

        public void Update()
        {
            lock (_lockObject)
            {
                // Spawn nowych celów
                if (_targets.Count < MAX_TARGETS && new Random().NextDouble() < SPAWN_CHANCE)
                {
                    SpawnNewTarget();
                }

                // Aktualizacja pozycji celów
                foreach (var target in _targets)
                {
                    target.UpdatePosition();
                }

                // Usuwanie celów, które były zestrzelone przez 4 tury
                _targets.RemoveAll(t => t.IsDestroyed && t.TurnsAfterDestruction <= 0);
            }
        }

        private void SpawnNewTarget()
        {
            var target = Target.CreateRandom(_nextTargetId++);
            _targets.Add(target);
        }

        public void DestroyTarget(int targetIndex)
        {
            lock (_lockObject)
            {
                var targetToDestroy = _targets
                    .Where(t => !t.IsDestroyed)
                    .Skip(targetIndex)
                    .FirstOrDefault();

                if (targetToDestroy != null)
                {
                    targetToDestroy.IsDestroyed = true;
                    targetToDestroy.TurnsAfterDestruction = 4;
                }
            }
        }
    }
} 