using Microsoft.Xna.Framework.Audio;
using TGC.MonoGame.TP.src.Core;

namespace TGC.MonoGame.TP.src.Physics
{
    /// <summary>
    ///     Que pasa cuando un auto de trafico toca algo. Solo reacciona al jugador
    ///     (trafico contra trafico se ignora: ya se separan solos por los carriles).
    /// </summary>
    internal class TrafficCollisionResponse : ICollisionResponse
    {
        private readonly TrafficCar _car;
        private readonly Entity _player;
        private readonly float _damage;
        private SoundEffect _sfx;

        public TrafficCollisionResponse(TrafficCar car, Entity player, float damage,SoundEffect sfx)
        {
            _car = car;
            _player = player;
            _damage = damage;
            _sfx = sfx;
        }

        public void OnCollision(Entity self, Entity other)
        {
            if (_car.Crashed || !ReferenceEquals(other, _player)) return;
            if (_sfx != null) _sfx.Play();

            other.Damageable?.ApplyDamage(_damage);
            _car.Crash(other.Transform.Position);
        }
    }
}
