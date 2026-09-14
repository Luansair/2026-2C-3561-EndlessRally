using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TGC.MonoGame.TP
{
    internal class CompositeShape : Shape
    {
        private readonly Shape[] _shapes;

        public CompositeShape(params Shape[] shapes)
        {
            _shapes = shapes;
        }

        public override bool Contains(Vector3 position) {
            foreach (var shape in _shapes)
            {
                if (shape.Contains(position))
                {
                    return true;
                }
            }
            return false;
        }

        public override Vector3 GetRandomPosition(Random random)
        {
            // Randomly select one of the shapes
            int index = random.Next(_shapes.Length);
            return _shapes[index].GetRandomPosition(random);
        }
    }
}