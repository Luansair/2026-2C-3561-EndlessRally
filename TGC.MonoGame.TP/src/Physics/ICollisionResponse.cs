using TGC.MonoGame.TP.src.Core;
using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.src.Animation;

namespace TGC.MonoGame.TP.src.Physics
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
            if (other.Render.Animator is CollisionAnimation crashAnim)  crashAnim.StartCrash(Vector3.Normalize(other.Transform.Position - self.Transform.Position));
            if (IsFrontCollision(self, other)) other.Damageable?.ApplyDamage(500f);
        }
        private static bool IsFrontCollision(Entity self, Entity other)
        {
            if (self.Collider == null || other.Collider == null)
                return false;
            if (other.Collider.Shape is NullShape) return false;

            // Zona frontal (25% delantero) de la caja del otro, en su espacio local.
            // Se testea como OBB contra OBB, asi la rotacion relativa no agranda la zona.
            var ob = other.Collider.Shape.LocalBox;
            float frontEndZ = ob.Min.Z + (ob.Max.Z - ob.Min.Z) * 0.25f;
            var frontZone = new AABBShape(new BoundingBox(ob.Min, new Vector3(ob.Max.X, ob.Max.Y, frontEndZ)));

            return self.Collider.Shape.Intersects(frontZone, self.Transform.World, other.Transform.World);
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
    internal class CollectibleCollisionResponse : ICollisionResponse
    {
        public void OnCollision(Entity self, Entity other)
        {
            if (other.Render.Animator is CollisionAnimation crashAnim)  crashAnim.StartCollect();
        }
    }  
}
