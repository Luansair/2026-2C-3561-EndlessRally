using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TGC.MonoGame.TP
{
    internal class World
    {
        public List<Entity> MovableEntities { get; } = new();
        public List<Entity> StaticEntities { get; } = new();
        private readonly CollisionSystem _collisionSystem = new();
        private readonly CollectibleSystem _collectibleSystem = new();
        public void Update(GameTime gameTime, Entity playerEntity, ICollectibleTarget playerTarget)
        {
            _collisionSystem.ResolveAll(MovableEntities);
            _collisionSystem.ResolveCross(MovableEntities, StaticEntities);

            _collectibleSystem.Update(MovableEntities, playerEntity, playerTarget);
            _collectibleSystem.Update(StaticEntities, playerEntity,  playerTarget);

            float despawnDist = 1600f;
            
            MovableEntities.RemoveAll(e => !e.Active || Vector3.Distance(e.Transform.Position, playerEntity.Transform.Position) > despawnDist);
            StaticEntities.RemoveAll(e => !e.Active || Vector3.Distance(e.Transform.Position, playerEntity.Transform.Position) > despawnDist);
        }

        public void Draw(Effect effect, Matrix view, Matrix proj)
        {
            foreach (var e in MovableEntities)
            {
                if (e.Active) e.Draw(effect, view, proj);
            }
            foreach (var e in StaticEntities)
            {
                if (e.Active) e.Draw(effect, view, proj);
            }
        }

        public void Add(Entity e)
        {
            if (e.IsMovable) MovableEntities.Add(e);
            else StaticEntities.Add(e);
        }
    }
}
