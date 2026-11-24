using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using System;
using TGC.MonoGame.TP.src.Collectibles;

namespace TGC.MonoGame.TP.src.Core
{
    internal class PlayerController : ICollectibleTarget
    {
        public Entity Vehicle { get; }
        public int Score { get; private set; }
        public SoundEffect _sfxLose;
        public PlayerController(Entity vehicle, SoundEffect sfx)
        {
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
        public float CurrentFriction { get; internal set; } = 1f;
        public float SnowFactor { get; set; } = 0f;
        private Vector3 _moveDirection = Vector3.Forward;
        private bool _hasMoveDirection = false;

        private float modifConsumoComb = 30f;
        private KeyboardState previousKeyboardState = Keyboard.GetState();
        private KeyboardState currentKeyboardState = Keyboard.GetState();

        public void Update(GameTime gameTime, KeyboardState keyboard)
        {
            float DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var t = Vehicle.Transform;
            Vector3 startPosition = t.Position;
            var direction = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(t.RotationY));
            float speed = Vehicle.VehicleStats.Acceleration * DeltaTime * 0.5f;

            if (!_hasMoveDirection || Vector3.Dot(_moveDirection, direction) < -0.5f)
            {
                _moveDirection = direction;
                _hasMoveDirection = true;
            }

            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            if (currentKeyboardState.IsKeyDown(Keys.G) && previousKeyboardState.IsKeyUp(Keys.G)) godmode = !godmode;

            if (godmode)
            {
                Vehicle.Damageable.Current = 999;
                Vehicle.Fuel.Current = 999;
                _moveDirection = direction;

                if (keyboard.IsKeyDown(Keys.Up)) t.Position += direction * speed;
                if (keyboard.IsKeyDown(Keys.Down)) t.Position -= direction * speed;
                if (keyboard.IsKeyDown(Keys.Left)) t.RotationY += MathHelper.ToRadians(Vehicle.VehicleStats.TurnSpeed) * DeltaTime;
                if (keyboard.IsKeyDown(Keys.Right)) t.RotationY -= MathHelper.ToRadians(Vehicle.VehicleStats.TurnSpeed) * DeltaTime;
            }
            else
            {
                if (IsDead) return;

                if (CurrentHealth <= 0 || CurrentFuel <= 0)
                {
                    CurrentSpeed = 0f;
                    IsDead = true;
                    _sfxLose.Play();
                    return;
                }

                float slipFactor = SnowFactor > 0f
                ? MathHelper.Clamp(SnowFactor, 0f, 1f)
                : MathHelper.Clamp((CurrentFriction - 1.0f) * 2f, 0f, 1f);

                float turnMultiplier = MathHelper.Lerp(1.0f, 0.62f, slipFactor); // Tierra 0.81f   Nieve 0.62f
                float lateralGrip    = MathHelper.Lerp(15.0f, 2.8f, slipFactor); // Tierra 8.9f    Nieve 2.8f
                float brakeResponse  = MathHelper.Lerp(1.0f, 0.45f, slipFactor); // Tierra 0.72f   Nieve 0.45f

                dificultad = (float)Math.Pow(Score / 100, 2) * 0.1f + 1;

                if (keyboard.IsKeyDown(Keys.S))
                {
                    slow = Math.Min(slow + (2.0f * brakeResponse * speed * DeltaTime), speed * 0.75f);
                    modifConsumoComb = 60f;
                }
                else
                {
                    slow = Math.Max(slow - (3.0f * speed * DeltaTime), 0f);
                    modifConsumoComb = 30f;
                }
                float effectiveTurnSpeed = Vehicle.VehicleStats.TurnSpeed * turnMultiplier;
                if (keyboard.IsKeyDown(Keys.A))
                {
                    t.RotationY += MathHelper.ToRadians(effectiveTurnSpeed) * DeltaTime;
                }
                if (keyboard.IsKeyDown(Keys.D))
                {
                    t.RotationY -= MathHelper.ToRadians(effectiveTurnSpeed) * DeltaTime;
                }

                //recalcula hacia donde apunta el frente del auto al girar
                direction = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(t.RotationY));

                //inercia, la trayectoria real tarda en alinearse en nieve
                _moveDirection = Vector3.Lerp(_moveDirection, direction, MathHelper.Clamp(lateralGrip * DeltaTime, 0f, 1f));
                if (_moveDirection.LengthSquared() > 0.0001f)
                    _moveDirection.Normalize();
                else
                    _moveDirection = direction;

                //avanzar siguiendo _moveDirection (patinando) en lugar de direction
                t.Position += _moveDirection * Math.Max(speed - slow, speed / 2) * dificultad * CurrentFriction;
                this.Vehicle.Fuel.Consume(this.Vehicle.VehicleStats.FuelConsumption * DeltaTime * modifConsumoComb);
            }

            CurrentSpeed = DeltaTime > 0f ? Vector3.Distance(startPosition, t.Position) / DeltaTime : 0f;
        }

        public void AddScore(int points) => Score += points;
        public void AddFuel(float amount) => Vehicle.Fuel?.Refill(amount);
        public void AddHealth(float amount) => Vehicle.Damageable?.Heal(amount);
        public void ApplyDamage(float amount) => Vehicle.Damageable?.ApplyDamage(amount);
    }
}