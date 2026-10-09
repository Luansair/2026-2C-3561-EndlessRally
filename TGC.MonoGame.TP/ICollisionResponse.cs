
namespace TGC.MonoGame.TP
{
    internal interface ICollisionResponse
    {
        void OnCollision(Entity self, Entity other);
    }

    // Utilizado para decoraciones y otros elementos que no se consumen al chocar con ellos
    internal class DamageResponse : ICollisionResponse
    {
        private readonly float _damage;
        public DamageResponse(float damage) { _damage = damage; }
        public void OnCollision(Entity self, Entity other)
        {
            other.Damageable?.ApplyDamage(_damage);
        }
    }
    internal class NullCollisionResponse : ICollisionResponse
    {
        public static readonly NullCollisionResponse Instance = new();
        public void OnCollision(Entity self, Entity other)
        {
            // No hace nada
        }
    }
}
