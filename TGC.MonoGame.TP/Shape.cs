using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TGC.MonoGame.TP
{
    internal abstract class Shape
    {
        public abstract bool Contains(Vector3 position);
        /// <summary>
        ///     Returns a random position within the shape using the provided Random instance.
        /// </summary>
        public abstract Vector3 GetRandomPosition(Random random);
    }

    internal class RectangleShape : Shape
    {
        private Vector3 center;
        private float width;
        private float depth;

        public RectangleShape(Vector3 center, float width, float depth)
        {
            this.center = center;
            this.width = width;
            this.depth = depth;
        }

        public override bool Contains(Vector3 position)
        {
            var halfWidth = width / 2;
            var halfDepth = depth / 2;
            return position.X >= center.X - halfWidth && position.X <= center.X + halfWidth &&
                   position.Z >= center.Z - halfDepth && position.Z <= center.Z + halfDepth;
        }

        public override Vector3 GetRandomPosition(Random random)
        {
            var halfWidth = width / 2;
            var halfDepth = depth / 2;
            return new Vector3(
                center.X + random.NextSingle() * width - halfWidth,
                center.Y,
                center.Z + random.NextSingle() * depth - halfDepth
            );
        }
    }
}
