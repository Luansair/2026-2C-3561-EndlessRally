using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP.src.Unused
{
    internal class RoadSegmentContext
    {
        public Matrix World { get; init; }
        public PlacementArea RoadArea { get; init; }
        public IReadOnlyList<PlacementArea> DecorationAreas { get; init; }
    }
}
