using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP
{
    internal class PlayerController : ICollectibleTarget
    {
        public Entity Vehicle { get; }
        public int Score { get; private set; }

        public PlayerController(Entity vehicle) { Vehicle = vehicle; }

        public float CurrentHealth => Vehicle.Damageable?.Current ?? 0f;
        public float CurrentFuel => Vehicle.Fuel?.Current ?? 0f;

        public void Update(GameTime gameTime, KeyboardState keyboard)
        {
            var t = Vehicle.Transform;
            var direction = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(t.RotationY));
            float speed = Vehicle.VehicleStats.Acceleration * gameTime.GetElapsedSeconds();

            // Manejo de inputs aca utilizando atributos de la entidad
        }

        public void AddScore(int points) => Score += points;
        public void AddFuel(float amount) => Vehicle.Fuel?.Refill(amount);
        public void AddHealth(float amount) => Vehicle.Damageable?.Heal(amount);
        public void ApplyDamage(float amount) => Vehicle.Damageable?.ApplyDamage(amount);
    }
}
