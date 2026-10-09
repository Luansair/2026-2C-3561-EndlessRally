using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace TGC.MonoGame.TP
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
                if (!e.Collider.Shape.Intersects(targetEntity.Collider.Shape, e.Transform.World, targetEntity.Transform.World)) continue;

                e.Collectible.ApplyTo(target);
                e.Active = false;
            }
        }
    }
}
