using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace TGC.MonoGame.TP
{
    
    internal class DecorationAreaFactory
    {
        private readonly List<DecorationGroup> _recipes;

        public DecorationAreaFactory(List<DecorationGroup> recipes)
        {
            _recipes = recipes;
        }

        public DecorationArea CreateFor(RoadPiece roadPiece, int chunkIndex)
        {
            float roadHalfWidth = 30f;
            float decorationMargin = 20f;
            float outerHalfWidth = 120f;

            float safeStart = roadHalfWidth + decorationMargin;
            float sideWidth = outerHalfWidth - safeStart;

            float rectangleMiddlePosX = safeStart + outerHalfWidth / 2f;

            var leftPos = new Vector3(-rectangleMiddlePosX, 0, 0);
            var rightPos = new Vector3(rectangleMiddlePosX, 0, 0);  
            Shape left = new RectangleShape(leftPos, sideWidth, roadPiece.offsetLocal.Z);
            Shape right = new RectangleShape(rightPos, sideWidth, roadPiece.offsetLocal.Z);

            Shape shape = new CompositeShape(left, right);
            Random random = new Random(chunkIndex);

            return new DecorationArea(shape, _recipes, random);
        }
    }
}
