using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TGC.MonoGame.TP
{
    internal class RoadChunk
    {
        private readonly int _index;
        private readonly RoadSegment _road;
        private readonly DecorationArea _decorations;
        // largo de la pieza en unidades de mundo (offsetLocal.Z), lo usa el shader para el fundido
        private readonly float _length;

        // Mezcla de superficies de este chunk
        public SurfaceBlend Surface { get; }

        // Exponemos la posición para que el Spawner sepa dónde está
        public Vector3 Position => _road.World.Translation;

        public RoadChunk(int index, Matrix world, RoadSegment road, DecorationArea decorations,
                         SurfaceBlend surface, float length)
        {
            _index = index;
            _road = road;
            _decorations = decorations;
            Surface = surface;
            _length = length;
        }

        public void Draw(Effect effect, Matrix view, Matrix projection)
        {
            Matrix w = _road.World;

            // Eje local +Z del chunk en mundo (OJO: Matrix.Forward es -Z, por eso se usa TransformNormal)
            Vector3 forward = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitZ, w));

            effect.Parameters["ChunkOrigin"]?.SetValue(w.Translation);
            effect.Parameters["ChunkForward"]?.SetValue(forward);
            effect.Parameters["ChunkLength"]?.SetValue(_length);
            effect.Parameters["SurfaceTypeA"]?.SetValue((float)Surface.From);
            effect.Parameters["SurfaceTypeB"]?.SetValue((float)Surface.To);
            effect.Parameters["BlendStart"]?.SetValue(Surface.T0);
            effect.Parameters["BlendEnd"]?.SetValue(Surface.T1);

            // RoadSegment.Draw activa UseSurfaceBlend solo en la parte de asfalto
            _road.Draw(effect, view, projection);

            effect.Parameters["UseSurfaceBlend"]?.SetValue(0f);
            _decorations.DrawRelativeTo(_road.World, effect, view, projection);
        }
    }
}
