using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using TGC.MonoGame.TP.src.Core;

namespace TGC.MonoGame.TP.src.Physics
{
    internal class CollisionSystem
    {
        private const float MaxCheckDistance = 35f;
        private const float MaxCheckDistanceSq = MaxCheckDistance * MaxCheckDistance;

        // Narrow phase
        public void Resolve(Entity a, Entity b)
        {
            if (!a.Collider.Shape.Intersects(b.Collider.Shape, a.Transform.World, b.Transform.World)) return;

            a.Response?.OnCollision(a, b);
            b.Response?.OnCollision(b, a);
        }

        public void ResolveAll(List<Entity> entities)
        {
            int count = entities.Count;
            for (int i = 0; i < count; i++)
            {
                var a = entities[i];
                if (!a.Active || a.Collider == null) continue;
                var posA = a.Transform.Position;

                for (int j = i + 1; j < count; j++)
                {
                    var b = entities[j];
                    if (!b.Active || b.Collider == null) continue;
                    if (a.Response == null && b.Response == null) continue;

                    var posB = b.Transform.Position;
                    float dx = posA.X - posB.X;
                    if (dx > MaxCheckDistance || dx < -MaxCheckDistance) continue;

                    float dz = posA.Z - posB.Z;
                    if (dz > MaxCheckDistance || dz < -MaxCheckDistance) continue;
                    if (dx * dx + dz * dz > MaxCheckDistanceSq) continue;
                    
                    Resolve(a, b);
                }
            }
        }

        public void ResolveCross(List<Entity> entitiesA, List<Entity> entitiesB)
        {
            int countA = entitiesA.Count;
            int countB = entitiesB.Count;

            for (int i = 0; i < countA; i++)
            {
                var a = entitiesA[i];
                if (!a.Active || a.Collider == null) continue;

                var posA = a.Transform.Position;

                for (int j = 0; j < countB; j++)
                {
                    var b = entitiesB[j];
                    if (!b.Active || b.Collider == null) continue;
                    if (a.Response == null && b.Response == null) continue;

                    var posB = b.Transform.Position;
                    float dx = posA.X - posB.X;
                    if (dx > MaxCheckDistance || dx < -MaxCheckDistance) continue;

                    float dz = posA.Z - posB.Z;
                    if (dz > MaxCheckDistance || dz < -MaxCheckDistance) continue;
                    if (dx * dx + dz * dz > MaxCheckDistanceSq) continue;

                    Resolve(a, b);
                }
            }
        }
    }
}