using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Runtime.CompilerServices;
using TGC.MonoGame.TP.src.Collectibles;

namespace TGC.MonoGame.TP.src.Core
{
    internal class PlayerController : ICollectibleTarget
    {
        public Entity Vehicle { get; }
        public int Score { get; private set; }

        public PlayerController(Entity vehicle) { Vehicle = vehicle; }

        public float CurrentHealth => Vehicle.Damageable?.Current ?? 0f;
        public float CurrentFuel => Vehicle.Fuel?.Current ?? 0f;
        private bool godmode = false;
        public void Update(GameTime gameTime, KeyboardState keyboard)
        {
            if (keyboard.IsKeyDown(Keys.G)) godmode = !godmode;
            if (godmode) { Vehicle.Damageable.Current = 999; Vehicle.Fuel.Current = 999;}
            if (CurrentHealth <= 0 || CurrentFuel <= 0)
            {
                return;
            }
            var t = Vehicle.Transform;
            var direction = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(t.RotationY));
            float speed = Vehicle.VehicleStats.Acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyboard.IsKeyDown(Keys.W))
            {
                t.Position += direction * speed;
                this.Vehicle.Fuel.Consume(this.Vehicle.VehicleStats.FuelConsumption * (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (keyboard.IsKeyDown(Keys.S))
            {
                t.Position -= direction * speed;
                this.Vehicle.Fuel.Consume(this.Vehicle.VehicleStats.FuelConsumption * (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (keyboard.IsKeyDown(Keys.A))
            {
                t.RotationY += MathHelper.ToRadians(100f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (keyboard.IsKeyDown(Keys.D))
            {
                t.RotationY -= MathHelper.ToRadians(100f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            
          
            // Manejo de inputs aca utilizando atributos de la entidad
        }

        public void AddScore(int points) => Score += points;
        public void AddFuel(float amount) => Vehicle.Fuel?.Refill(amount);
        public void AddHealth(float amount) => Vehicle.Damageable?.Heal(amount);
        public void ApplyDamage(float amount) => Vehicle.Damageable?.ApplyDamage(amount);
    }
}
