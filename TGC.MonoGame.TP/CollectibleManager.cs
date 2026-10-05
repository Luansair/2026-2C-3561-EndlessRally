using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace TGC.MonoGame.TP
{
    internal class CollectibleManager
    {
        private List<Collectible> activeCollectibles;

        public CollectibleManager()
        {
            activeCollectibles = new List<Collectible>();
        }

        public void Add(Collectible collectible)
        {
            activeCollectibles.Add(collectible);
        }

        public void Update(Vehiculo car, float despawnDistance)
        {
            checkCollisions(car);
            activeCollectibles.RemoveAll(c => c.Collected || Vector3.Distance(c.pos, car.pos) > despawnDistance);
        }

        public void Draw(GraphicsDevice graphics, Effect effect, Matrix view, Matrix projection, GameTime gameTime)
        {
            foreach (Collectible c in activeCollectibles)
            {
                if (c.Collected) continue;
                c.Draw(effect, view, projection, gameTime);

                var hitWorld = Matrix.CreateScale(c.modelI.Scale) * Matrix.CreateTranslation(c.pos);
                GizmoPrimitives.DrawBoundingBox(graphics, effect, c.hitbox.Min, c.hitbox.Max, hitWorld, view, projection, Color.Blue);
            }
        }
        private void checkCollisions(Vehiculo car)
        {
            foreach (Collectible c in activeCollectibles)
            {
                if (c.Collected) continue;
                c.tryCollect(car);
            }
        }
    }
}
