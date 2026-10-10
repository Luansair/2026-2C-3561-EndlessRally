using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using TGC.MonoGame.TP.src.Core;

namespace TGC.MonoGame.TP.src.Unused
{
    // Represents an area where objects can be placed, defined by a shape.
    internal class PlacementArea
    {
        private readonly Shape _shape;
        public PlacementArea(Shape shape)
        {
            _shape = shape;
        }

        public Vector3 GetRandomPosition(Random random)
        {
            return _shape.GetRandomPosition(random);
        }
    }
}
