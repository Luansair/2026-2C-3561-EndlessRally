using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TGC.MonoGame.TP.src.Core;

namespace TGC.MonoGame.TP.src.Collectibles
{
    internal class CollectibleSystem
    {
        public void Update(IEnumerable<Entity> entities, Entity targetEntity, ICollectibleTarget target)
        {
            if (targetEntity.Collider == null) return;

            foreach (var e in entities)
            {
                if (!e.Active || e.Collectible == null) continue;
                if (e.Collider == null) continue;
                if (Vector3.DistanceSquared(e.Transform.Position, targetEntity.Transform.Position) > 2500f) continue;
                if (!e.Collider.Shape.Intersects(targetEntity.Collider.Shape, e.Transform.World, targetEntity.Transform.World)) continue;

                e.Collectible.ApplyTo(target);
                e.Active = false;
            }
        }
    }
}
