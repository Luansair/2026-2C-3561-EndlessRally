using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TGC.MonoGame.TP.src.Collectibles;
using TGC.MonoGame.TP.src.Physics;
using System.Linq;
namespace TGC.MonoGame.TP.src.Core
{
    internal class World
    {
        public List<Entity> MovableEntities { get; } = new();
        public List<Entity> StaticEntities { get; } = new();
        //public List<Entity> TrafficEntities { get;} = new();
        // F1 en el juego: dibuja las bounding box de colision (gizmos)
        public bool DebugBounds { get; set; }

        private readonly CollisionSystem _collisionSystem = new();
        private readonly CollectibleSystem _collectibleSystem = new();
        public void Update(GameTime gameTime, Entity playerEntity, ICollectibleTarget playerTarget)
        {
            _collisionSystem.ResolveAll(MovableEntities);
            _collisionSystem.ResolveCross(MovableEntities, StaticEntities);

            _collectibleSystem.Update(MovableEntities, playerEntity, playerTarget);
            _collectibleSystem.Update(StaticEntities, playerEntity,  playerTarget);

            float despawnDist = 800f;
            
            MovableEntities.RemoveAll(e => !e.Active || Vector3.Distance(e.Transform.Position, playerEntity.Transform.Position) > despawnDist);
            StaticEntities.RemoveAll(e => !e.Active || Vector3.Distance(e.Transform.Position, playerEntity.Transform.Position) > despawnDist);
        }

        public void Draw(Effect effect, Matrix view, Matrix proj)
        {
            var frustum = new BoundingFrustum(view * proj);

            // 1) camino, decoraciones, etc: con nieve acumulada
            DrawPass(effect, view, proj, frustum, noSnowPass: false);

            // 2) auto y coleccionables: sin nieve encima (igual que en la v1)
            effect.Parameters["SnowCover"]?.SetValue(0f);
            DrawPass(effect, view, proj, frustum, noSnowPass: true);
        }

        private void DrawPass(Effect effect, Matrix view, Matrix proj, BoundingFrustum frustum, bool noSnowPass)
        {
            foreach (var e in MovableEntities) DrawIfVisible(e, effect, view, proj, frustum, noSnowPass);
            foreach (var e in StaticEntities) DrawIfVisible(e, effect, view, proj, frustum, noSnowPass);
        }

        private void DrawIfVisible(Entity e, Effect effect, Matrix view, Matrix proj, BoundingFrustum frustum, bool noSnowPass)
        {
            if (!e.Active) return;
            if ((e.IsMovable || e.Collectible != null) != noSnowPass) return;
            if (frustum.Contains(e.GetWorldSphere()) == ContainmentType.Disjoint) return;
            e.Draw(effect, view, proj);

            if (DebugBounds && e.Collider != null && e.Collider != Collider.None)
            {
                var box = e.Collider.Shape.LocalBox;
                GizmoPrimitives.DrawBoundingBox(effect.GraphicsDevice, effect, box.Min, box.Max,
                    e.Transform.World, view, proj, Color.Blue);
            }
        }

        public void Add(Entity e)
        {
            if (e.IsMovable) MovableEntities.Add(e);
            else StaticEntities.Add(e);
        }
    
}
}