using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using TGC.MonoGame.TP.src.Collectibles;

namespace TGC.MonoGame.TP.src.Core
{
    internal class PlayerController : ICollectibleTarget
    {
        public Entity Vehicle { get; }
        public int Score { get; private set; }
        public SoundEffect _sfxLose;
        public PlayerController(Entity vehicle, SoundEffect sfx) { 
            Vehicle = vehicle;
            _sfxLose = sfx;
        }
        public bool IsDead = false;
        public float CurrentHealth => Vehicle.Damageable?.Current ?? 0f;
        public float CurrentFuel => Vehicle.Fuel?.Current ?? 0f;
        public bool godmode = false;
        private float slow = 0f;
        // Dificultad inicial (multiplica la velocidad). La usa el trafico para calcular su velocidad de referencia.
        public const float InitialDifficulty = 0.71f;
        private float dificultad = InitialDifficulty;

        // Velocidad real del auto en unidades/segundo (la usa el trafico para ser relativo al jugador)
        public float CurrentSpeed { get; private set; }
        private float modifConsumoComb = 0.5f;
        private KeyboardState previousKeyboardState = Keyboard.GetState();
        private KeyboardState currentKeyboardState = Keyboard.GetState();

        
        public void Update(GameTime gameTime, KeyboardState keyboard)
        {
            float DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            //Modificador de velocidad para que no salga super rapido el auto
            var t = Vehicle.Transform;
            Vector3 startPosition = t.Position;
            var direction = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(t.RotationY));
            float speed = Vehicle.VehicleStats.Acceleration * DeltaTime;

            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            if (currentKeyboardState.IsKeyDown(Keys.G) && previousKeyboardState.IsKeyUp(Keys.G)) godmode = !godmode;

            if (godmode)
            {
                Vehicle.Damageable.Current = 999; Vehicle.Fuel.Current = 999;
                if (keyboard.IsKeyDown(Keys.Up))
                {
                    t.Position += direction * speed;
                }
                if (keyboard.IsKeyDown(Keys.Down))
                {
                    t.Position -= direction * speed;
                }
                if (keyboard.IsKeyDown(Keys.Left))
                {
                    t.RotationY += MathHelper.ToRadians(Vehicle.VehicleStats.TurnSpeed) * DeltaTime;
                }
                if (keyboard.IsKeyDown(Keys.Right))
                {
                    t.RotationY -= MathHelper.ToRadians(Vehicle.VehicleStats.TurnSpeed) * DeltaTime;
                }
            }
            else
            {
                if (IsDead) return;   

                if (CurrentHealth <= 0 || CurrentFuel <= 0) { 
                    CurrentSpeed = 0f;
                    IsDead = true;
                    _sfxLose.Play(); 
                }

                dificultad += 0.01f * DeltaTime;
                if (keyboard.IsKeyDown(Keys.S))
                {
                    slow = Math.Min(slow + (2.0f * speed * DeltaTime), speed * 0.75f);
                }
                else
                {
                    slow = Math.Max(slow - (3.0f * speed * DeltaTime), 0f);
                }
                t.Position += direction * Math.Max(speed-slow, speed/2) * dificultad;
                this.Vehicle.Fuel.Consume(this.Vehicle.VehicleStats.FuelConsumption * DeltaTime * modifConsumoComb);
                if (keyboard.IsKeyDown(Keys.A))
                {
                    t.RotationY += MathHelper.ToRadians(Vehicle.VehicleStats.TurnSpeed) * DeltaTime;
                }
                if (keyboard.IsKeyDown(Keys.D))
                {
                    t.RotationY -= MathHelper.ToRadians(Vehicle.VehicleStats.TurnSpeed) * DeltaTime ;
                }
            }

            CurrentSpeed = DeltaTime > 0f ? Vector3.Distance(startPosition, t.Position) / DeltaTime : 0f;

            // Manejo de inputs aca utilizando atributos de la entidad
        }

        public void AddScore(int points) => Score += points;
        public void AddFuel(float amount) => Vehicle.Fuel?.Refill(amount);
        public void AddHealth(float amount) => Vehicle.Damageable?.Heal(amount);
        public void ApplyDamage(float amount) => Vehicle.Damageable?.ApplyDamage(amount);
    }
}
