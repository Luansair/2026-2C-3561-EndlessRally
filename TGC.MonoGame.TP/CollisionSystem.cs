using System.Collections;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace TGC.MonoGame.TP
{
    internal class CollisionSystem
    {
        private const float MaxCheckDistance = 100f;

        // Narrow phase. 
        // Take into consideration that Collectibles shouldn't have a Response (null), since their interaction is handled in Collectible System.
        public void Resolve(Entity a, Entity b)
        {
            if (a.Collider == null || b.Collider == null) return;
            if (!a.Collider.Shape.Intersects(b.Collider.Shape, a.Transform.World, b.Transform.World)) return;

            a.Response?.OnCollision(a, b);
            b.Response?.OnCollision(b, a);
        }

        // Mejorar performance con lo del chat. Resolve All para una misma lista y Resolve Cross para dos. Tomar cercanos.
        // Otra posible implementacion es que la lista se ordene segun la distancia al jugador aunq solo serviria para un objeto movil.
        // Todavia no se considera el caso q la entidad sea muy veloz y pueda atravesar a la otra.
        public void ResolveAll(List<Entity> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (!entities[i].Active) continue;
                if (entities[i].Collider == null) continue;
                if (entities[i].Response == null) continue;

                for (int j = i + 1; j < entities.Count; j++)
                {
                    if (!entities[j].Active) continue;
                    if (entities[j].Collider == null) continue;
                    if (entities[j].Response == null) continue;

                    var d = Vector3.DistanceSquared(entities[i].Transform.Position, entities[j].Transform.Position);
                    if (d > MaxCheckDistance * MaxCheckDistance) continue;

                    Resolve(entities[i], entities[j]);
                }
            }
        }

        public void ResolveCross(IEnumerable<Entity> entitiesA, IEnumerable<Entity> entitiesB)
        {
            foreach (var a in entitiesA)
            {
                if (!a.Active) continue;
                if (a.Collider == null) continue;
                if (a.Response == null) continue;

                foreach (var b in entitiesB)
                {
                    if (!b.Active) continue;
                    if (b.Collider == null) continue;
                    if (b.Response == null) continue;

                    var d = Vector3.DistanceSquared(a.Transform.Position, b.Transform.Position);
                    if (d > MaxCheckDistance * MaxCheckDistance) continue;

                    Resolve(a, b);
                }
            }
        }
    }
}
