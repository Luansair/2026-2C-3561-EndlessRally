using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using TGC.MonoGame.TP.src.Physics;
using TGC.MonoGame.TP.src.Road;
using TGC.MonoGame.TP.src.Vehicles;

namespace TGC.MonoGame.TP.src.Core
{
    /// <summary>Parametros del trafico procedural. Todo en unidades de mundo / segundos.</summary>
    internal sealed class TrafficConfig
    {
        public int MaxCars { get; init; } = 14;
        public float CarScale { get; init; } = 0.008f;     // misma escala que usa el auto del jugador
        public float LaneWidth { get; init; } = 2.5f;      // igual a los carriles de los coleccionables

        // Donde aparecen (distancia sobre el camino, adelante del jugador). La niebla tapa el "pop-in".
        public float SpawnAheadMin { get; init; } = 300f;
        public float SpawnAheadMax { get; init; } = 390f;
        public float HideDistance { get; init; } = 280f;  // no aparecer mas cerca que esto en linea recta
        public float PrefillAheadMin { get; init; } = 100f; // al arrancar la carrera ya hay autos en el camino

        // Donde desaparecen
        public float DespawnBehind { get; init; } = 80f;
        public float DespawnAhead { get; init; } = 520f;

        // Reglas para que el trafico sea justo
        public float MinSameLaneGap { get; init; } = 40f;  // separacion minima al aparecer, en el mismo carril
        public float RowWindow { get; init; } = 25f;       // nunca aparece una "pared" de 3 autos en esta ventana

        public float SpawnIntervalMin { get; init; } = 0.4f;
        public float SpawnIntervalMax { get; init; } = 1.1f;
        public float CrashDamage { get; init; } = 25f;
        public float MinReferenceSpeed { get; init; } = 25f;
    }

    /// <summary>
    ///     Genera, mueve y borra autos de trafico de forma procedural, siguiendo el camino que genera RoadSpawner:
    ///     aparecen adelante del jugador, van en su carril a distintas velocidades (el carril derecho es el mas lento),
    ///     frenan si tienen otro auto adelante y desaparecen cuando quedan atras.
    /// </summary>
    internal class TrafficController
    {
        public List<TrafficCar> Vehicles { get; } = new();

        private readonly VehiclePresets _presets;
        private readonly World _world;
        private readonly Entity _player;
        private readonly RoadSpawner _road;
        private readonly TrafficConfig _config;
        private readonly Random _random;

        private float _spawnTimer;
        private float _playerDistance;

        public TrafficController(VehiclePresets presets, World world, Entity player, RoadSpawner road,
            TrafficConfig config = null, Random random = null)
        {
            _presets = presets;
            _world = world;
            _player = player;
            _road = road;
            _config = config ?? new TrafficConfig();
            _random = random ?? new Random();

            _playerDistance = _road.GetDistanceAlongRoad(_player.Transform.Position);
            Prefill();
        }

        public void Update(GameTime gameTime, float playerSpeed)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt <= 0f) return;

            _playerDistance = _road.GetDistanceAlongRoad(_player.Transform.Position);

            // 1) Mover y borrar (de atras para adelante en la lista, asi se puede borrar mientras se recorre)
            for (int i = Vehicles.Count - 1; i >= 0; i--)
            {
                TrafficCar car = Vehicles[i];

                bool alive = car.Vehicle.Active
                    && car.Distance > _playerDistance - _config.DespawnBehind
                    && car.Distance < _playerDistance + _config.DespawnAhead
                    && car.Update(dt, ComputeTargetSpeed(car), _road);

                if (!alive)
                {
                    car.Vehicle.Active = false;     // World lo saca de sus listas en el proximo Update
                    Vehicles.RemoveAt(i);
                }
            }

            // 2) Generar autos nuevos adelante, de a poco
            _spawnTimer -= dt;
            if (_spawnTimer > 0f) return;

            _spawnTimer = Lerp(_config.SpawnIntervalMin, _config.SpawnIntervalMax, _random.NextSingle());
            if (Vehicles.Count < _config.MaxCars)
            {
                float reference = MathF.Max(playerSpeed, _config.MinReferenceSpeed);
                TrySpawn(Lerp(_config.SpawnAheadMin, _config.SpawnAheadMax, _random.NextSingle()), false, reference);
            }
        }

        // Al arrancar la carrera el camino ya esta generado: llenamos el tramo de adelante
        private void Prefill()
        {
            float reference = _player.VehicleStats.Acceleration * PlayerController.InitialDifficulty;
            int attempts = _config.MaxCars * 4;

            for (int i = 0; i < attempts && Vehicles.Count < _config.MaxCars; i++)
            {
                float ahead = Lerp(_config.PrefillAheadMin, _config.SpawnAheadMax, _random.NextSingle());
                TrySpawn(ahead, true, MathF.Max(reference, _config.MinReferenceSpeed));
            }
        }

        private bool TrySpawn(float ahead, bool prefill, float referenceSpeed)
        {
            float distance = _playerDistance + ahead;

            if (!_road.TrySampleRoad(distance, out RoadSample sample)) return false;
            if (sample.Type != RoadPieceType.STRAIGHT) return false;   // solo aparecen en rectas
            if (!prefill && Vector3.Distance(sample.Position, _player.Transform.Position) < _config.HideDistance)
                return false;   // todavia se veria aparecer: lo intentamos en el proximo ciclo

            int lane = PickLane(distance);
            if (lane == int.MinValue) return false;

            VehicleType type = PickType();
            Entity entity = _presets.Create(type);
            if (entity == null) return false;
            entity.Transform.Scale = _config.CarScale;

            // Carril derecho = lento, izquierdo = rapido (como una autopista). Los pesados son un poco mas lentos.
            float factor = lane switch
            {
                -1 => Lerp(0.50f, 0.65f, _random.NextSingle()),
                0 => Lerp(0.38f, 0.52f, _random.NextSingle()),
                _ => Lerp(0.28f, 0.40f, _random.NextSingle())
            };
            if (type == VehicleType.HeavyCar) factor *= 0.85f;

            var car = new TrafficCar(entity, lane, _config.LaneWidth, distance, referenceSpeed * factor);
            entity.Response = new TrafficCollisionResponse(car, _player, _config.CrashDamage);

            car.Update(0f, car.DesiredSpeed, _road);   // lo ubica en el camino antes de que se dibuje
            Vehicles.Add(car);
            _world.Add(entity);
            return true;
        }

        private VehicleType PickType()
        {
            int roll = _random.Next(100);
            if (roll < 30) return VehicleType.Taxi;
            if (roll < 55) return VehicleType.LightCar;
            if (roll < 85) return VehicleType.MediumCar;
            return VehicleType.HeavyCar;
        }

        // Elige un carril libre al azar; int.MinValue si ninguno sirve
        private int PickLane(float distance)
        {
            int first = _random.Next(3);
            for (int k = 0; k < 3; k++)
            {
                int lane = ((first + k) % 3) - 1;
                if (IsLaneFree(lane, distance)) return lane;
            }
            return int.MinValue;
        }

        private bool IsLaneFree(int lane, float distance)
        {
            int othersBlocked = 0;   // bit por cada otro carril ocupado en la misma "fila"

            foreach (var car in Vehicles)
            {
                float gap = MathF.Abs(car.Distance - distance);

                if (car.Lane == lane)
                {
                    if (gap < _config.MinSameLaneGap) return false;
                }
                else if (gap < _config.RowWindow)
                {
                    othersBlocked |= 1 << (car.Lane + 1);
                }
            }

            // Si los otros dos carriles ya estan ocupados en esta fila, este cerraria la pared: siempre dejamos un hueco
            bool wall = (othersBlocked & (othersBlocked - 1)) != 0;
            return !wall;
        }

        // Velocidad objetivo: la suya, salvo que tenga un auto adelante en su carril (entonces se acomoda a el)
        private float ComputeTargetSpeed(TrafficCar car)
        {
            if (car.Crashed) return 0f;

            TrafficCar leader = null;
            float leaderGap = float.MaxValue;

            foreach (var other in Vehicles)
            {
                if (other == car || other.Lane != car.Lane || other.Crashed) continue;

                float d = other.Distance - car.Distance;
                if (d > 0f && d < leaderGap)
                {
                    leaderGap = d;
                    leader = other;
                }
            }

            if (leader == null) return car.DesiredSpeed;

            float bumperGap = leaderGap - (car.Length + leader.Length) * 0.5f;
            float followDistance = car.Length * 2.5f;
            if (bumperGap >= followDistance) return car.DesiredSpeed;

            // Cuanto mas cerca, mas frena: tiende a quedarse a "followDistance" sin chocarlo
            float t = MathHelper.Clamp(bumperGap / followDistance, 0f, 1f);
            return MathF.Min(car.DesiredSpeed, leader.Speed * t);
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;
    }
}
