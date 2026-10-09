using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TGC.MonoGame.TP
{
    internal class RoadChunk
    {
        public Entity  Road { get; }
        public List<Entity> Decorations { get; }
        public SurfaceMaterial Surface { get; }
        public Vector3 Position => Road.Transform.Position;

        public RoadChunk(Entity road, List<Entity> decorations, SurfaceMaterial surface)
        {
            Road = road;
            Decorations = decorations;
            Surface = surface;
        }

        public void Draw(Effect effect, Matrix view, Matrix projection)
        {
            Road.Render.Draw(effect, view, projection, Road.Transform.World);
            foreach (var d in Decorations) d.Render.Draw(effect, view, projection, d.Transform.World);
        }
    }
}
