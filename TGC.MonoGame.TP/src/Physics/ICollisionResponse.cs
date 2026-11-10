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

            var otherBox = other.Collider.Shape.LocalBox;
            var selfCorners = self.Collider.Shape.LocalBox.GetCorners();
            var otherWorldInverse = Matrix.Invert(other.Transform.World);

            Vector3 selfMin = Vector3.Zero;
            Vector3 selfMax = Vector3.Zero;
            for (int i = 0; i < selfCorners.Length; i++)
            {
                var worldCorner = Vector3.Transform(selfCorners[i], self.Transform.World);
                var otherLocalCorner = Vector3.Transform(worldCorner, otherWorldInverse);
                if (i == 0)
                {
                    selfMin = otherLocalCorner;
                    selfMax = otherLocalCorner;
                }
                else
                {
                    selfMin = Vector3.Min(selfMin, otherLocalCorner);
                    selfMax = Vector3.Max(selfMax, otherLocalCorner);
                }
            }
            bool overlapsWidth = selfMax.X >= otherBox.Min.X && selfMin.X <= otherBox.Max.X;
            bool overlapsHeight = selfMax.Y >= otherBox.Min.Y && selfMin.Y <= otherBox.Max.Y;
            float frontEndZ = otherBox.Min.Z + (otherBox.Max.Z - otherBox.Min.Z) * 0.25f;
            bool overlapsFront = selfMax.Z >= otherBox.Min.Z && selfMin.Z <= frontEndZ;
            return overlapsWidth && overlapsHeight && overlapsFront;
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
