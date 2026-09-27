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
            // Si la pieza es una curva, dejamos el interior despejado para no tapar la visibilidad
            if (roadPiece.rotacionY != 0)
            {
                // Retornamos un área sin decoraciones en curvas
                return new DecorationArea(null, _recipes, new Random(chunkIndex));
            }

            float roadHalfWidth = 5.0f;       // Ancho medio de la calzada con cordón
            float decorationMargin = 6.0f;    // Margen amplio para compensar el radio de copas y rocas
            float outerHalfWidth = 55.0f;     // Límite exterior del bosque

            float safeStart = roadHalfWidth + decorationMargin;
            float sideWidth = outerHalfWidth - safeStart;

            float rectangleMiddlePosX = safeStart + (sideWidth / 2f);
            float centerZ = roadPiece.offsetLocal.Z / 2f; // Centrado en el largo de la pieza

            var leftPos = new Vector3(-rectangleMiddlePosX, 0, centerZ);
            var rightPos = new Vector3(rectangleMiddlePosX, 0, centerZ);

            Shape left = new RectangleShape(leftPos, sideWidth, roadPiece.offsetLocal.Z);
            Shape right = new RectangleShape(rightPos, sideWidth, roadPiece.offsetLocal.Z);

            Shape shape = new CompositeShape(left, right);
            Random random = new Random(chunkIndex);

            return new DecorationArea(shape, _recipes, random);
        }
    }
}
