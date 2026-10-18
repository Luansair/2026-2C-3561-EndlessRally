using Microsoft.Xna.Framework;
using System;
using TGC.MonoGame.TP.src.Physics;
using TGC.MonoGame.TP.src.Road;

namespace TGC.MonoGame.TP.src.Core
{
    /// <summary>
    ///     Un auto de trafico. No guarda "x, y, z": guarda cuanto recorrio sobre el camino (Distance)
    ///     y en que carril va (Lane). La posicion sale de pedirle al camino ese punto y correrse hacia el costado,
    ///     asi se mantiene en su carril aunque haya curvas.
    /// </summary>
    internal class TrafficCar
    {
        private const float CrashDriftSpeed = 7f;   // unidades/s hacia el costado al ser chocado
        private const float CrashSpinSpeed = 5f;    // rad/s
        public const float CrashLifetime = 2.5f;    // segundos hasta que desaparece

        public Entity Vehicle { get; }
        /// <summary>Distancia recorrida sobre el eje del camino.</summary>
        public float Distance { get; private set; }
        /// <summary>-1 = izquierda, 0 = centro, 1 = derecha (segun el sentido de marcha).</summary>
        public int Lane { get; }
        public float Speed { get; private set; }
        public float DesiredSpeed { get; }
        /// <summary>Largo aproximado del auto en unidades de mundo.</summary>
        public float Length { get; }
        public bool Crashed { get; private set; }

        private readonly float _laneOffset;
        private Vector3 _lastRight = Vector3.Right;
        private float _crashSide;
        private float _crashTime;
        private float _crashOffset;
        private float _crashYaw;

        public TrafficCar(Entity vehicle, int lane, float laneWidth, float distance, float desiredSpeed)
        {
            Vehicle = vehicle;
            Lane = lane;
            _laneOffset = lane * laneWidth;
            Distance = distance;
            DesiredSpeed = desiredSpeed;
            Speed = desiredSpeed;

            // Largo del auto = lado mas largo de su caja de colision ya escalada
            var box = vehicle.Render.ModelInfo.LocalBounds;
            Length = MathF.Max(box.Max.X - box.Min.X, box.Max.Z - box.Min.Z) * vehicle.Transform.Scale;
        }

        /// <summary>Avanza el auto hacia la velocidad pedida. Devuelve false si hay que sacarlo del juego.</summary>
        public bool Update(float dt, float targetSpeed, RoadSpawner road)
        {
            float response = Crashed ? 2.5f : 6f;
            Speed += (targetSpeed - Speed) * (1f - MathF.Exp(-response * dt));
            Distance += Speed * dt;

            if (Crashed)
            {
                _crashTime += dt;
                _crashOffset += _crashSide * CrashDriftSpeed * dt;
                _crashYaw += _crashSide * CrashSpinSpeed * dt;
                if (_crashTime > CrashLifetime) return false;
            }

            if (!road.TrySampleRoad(Distance, out RoadSample sample))
                return false;

            // Derecha del conductor = forward x up (en XNA, mirando hacia -Z la derecha es +X)
            _lastRight = Vector3.Cross(sample.Forward, Vector3.Up);

            var t = Vehicle.Transform;
            t.Position = sample.Position + _lastRight * (_laneOffset + _crashOffset);
            // El modelo mira hacia -Z cuando RotationY = 0
            t.RotationY = MathF.Atan2(-sample.Forward.X, -sample.Forward.Z) + _crashYaw;
            return true;
        }

        /// <summary>El jugador lo choco: sale despedido hacia el lado contrario y deja de colisionar.</summary>
        public void Crash(Vector3 otherPosition)
        {
            if (Crashed) return;
            Crashed = true;
            Vehicle.Collider = Collider.None;   // un solo golpe por auto

            float side = Vector3.Dot(Vehicle.Transform.Position - otherPosition, _lastRight);
            _crashSide = side >= 0f ? 1f : -1f;
        }
    }
}
