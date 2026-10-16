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
            //Modificador de velocidad para que no salga super rapido el auto
            float modificadorVelocidad = 0.23f;
            var t = Vehicle.Transform;
            var direction = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(t.RotationY));
            float speed = Vehicle.VehicleStats.Acceleration * modificadorVelocidad * (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (keyboard.IsKeyDown(Keys.G)) godmode = !godmode;
            if (godmode) { 
                Vehicle.Damageable.Current = 999; Vehicle.Fuel.Current = 999;
                if (keyboard.IsKeyDown(Keys.Up))
                {
                    t.Position += direction * speed;
                    this.Vehicle.Fuel.Consume(this.Vehicle.VehicleStats.FuelConsumption * 0.01f * (float)gameTime.ElapsedGameTime.TotalSeconds);
                }
                if (keyboard.IsKeyDown(Keys.Down))
                {
                    t.Position -= direction * speed;
                    this.Vehicle.Fuel.Consume(this.Vehicle.VehicleStats.FuelConsumption * 0.01f * (float)gameTime.ElapsedGameTime.TotalSeconds);
                }
                if (keyboard.IsKeyDown(Keys.Left))
                {
                    t.RotationY += MathHelper.ToRadians(Vehicle.VehicleStats.TurnSpeed) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                if (keyboard.IsKeyDown(Keys.Right))
                {
                    t.RotationY -= MathHelper.ToRadians(Vehicle.VehicleStats.TurnSpeed) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                } 
            else {  
                    t.Position += direction * speed;
                    this.Vehicle.Fuel.Consume(this.Vehicle.VehicleStats.FuelConsumption * 0.01f * (float)gameTime.ElapsedGameTime.TotalSeconds);
                    if (keyboard.IsKeyDown(Keys.A))
                    {
                        t.RotationY += MathHelper.ToRadians(Vehicle.VehicleStats.TurnSpeed) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    if (keyboard.IsKeyDown(Keys.D))
                    {
                        t.RotationY -= MathHelper.ToRadians(Vehicle.VehicleStats.TurnSpeed) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    if (CurrentHealth <= 0 || CurrentFuel <= 0){return;}
                }
            
            
            
            
            
            
            
            
          
            // Manejo de inputs aca utilizando atributos de la entidad
        }

        public void AddScore(int points) => Score += points;
        public void AddFuel(float amount) => Vehicle.Fuel?.Refill(amount);
        public void AddHealth(float amount) => Vehicle.Damageable?.Heal(amount);
        public void ApplyDamage(float amount) => Vehicle.Damageable?.ApplyDamage(amount);
    }
}
