using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TGC.MonoGame.TP.src.Core;

namespace TGC.MonoGame.TP.src.Road
{
    internal class RoadChunk
    {
        public Entity  Road { get; }
        public List<Entity> Decorations { get; }
        public List<Entity> Cars { get; } 
        public SurfaceMaterial Surface { get; }
        public RoadPieceType Type { get; }
        public Vector3 Position => Road.Transform.Position;
        public SurfaceBlend Blend => Road.Blend ?? default;

        public RoadChunk(Entity road, List<Entity> decorations, SurfaceMaterial surface, RoadPieceType type, List<Entity> cars = null)
        {
            Road = road;
            Decorations = decorations;
            Surface = surface;
            Type = type;
            Cars = cars;
        }

        public void Draw(Effect effect, Matrix view, Matrix projection)
        {
            Road.Render.Draw(effect, view, projection, Road.Transform.World);
            foreach (var d in Decorations) d.Render.Draw(effect, view, projection, d.Transform.World);
        }
    }
}
