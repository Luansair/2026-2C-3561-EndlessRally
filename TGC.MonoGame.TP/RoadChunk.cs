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

        // Exponemos la posición para que el Spawner sepa dónde está
        public Vector3 Position => _road.World.Translation;

        public RoadChunk(int index, Matrix world, RoadSegment road, DecorationArea decorations)
        {
            _index = index;
            _road = road;
            _decorations = decorations;
        }

        public void Draw(Effect effect, Matrix view, Matrix projection)
        {
            _road.Draw(effect, view, projection);
            _decorations.DrawRelativeTo(_road.World, effect, view, projection);
        }
    }
}
